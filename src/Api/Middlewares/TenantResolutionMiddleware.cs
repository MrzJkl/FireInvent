using FireInvent.Contract;
using FireInvent.Database;
using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace FireInvent.Api.Middlewares;

/// <summary>
/// Middleware that resolves the active tenant per request based on Keycloak Organizations.
/// The JWT contains a list of organizations the user belongs to; the client must specify the active tenant via header.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;
    private const string TenantCachePrefix = "tenant_by_id";
    private static readonly TimeSpan TenantCacheExpiration = TimeSpan.FromMinutes(15);
    private const string TenantHeaderName = "X-Tenant-Id";
    private const string OrganizationsClaimName = "organization"; // per provided token

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        TenantProvider tenantProvider,
        AppDbContext dbContext,
        IMemoryCache cache)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        try
        {
            // Read organizations from claim. Structure: JSON object { OrgName: { id: GUID }, ... }
            var organizationsClaim = context.User.Claims.FirstOrDefault(c => c.Type == OrganizationsClaimName);
            if (organizationsClaim == null || string.IsNullOrWhiteSpace(organizationsClaim.Value))
            {
                _logger.LogWarning("No organizations claim found in JWT token.");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid token: missing organizations claim.");
                return;
            }

            List<Guid> tenantIdsFromToken = new();
            try
            {
                using var doc = JsonDocument.Parse(organizationsClaim.Value);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    throw new FormatException("Organizations claim root is not an object.");

                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    var value = prop.Value;
                    if (value.ValueKind == JsonValueKind.Object && value.TryGetProperty("id", out var idProp))
                    {
                        var idStr = idProp.GetString();
                        if (!string.IsNullOrWhiteSpace(idStr) && Guid.TryParse(idStr, out var gid))
                        {
                            tenantIdsFromToken.Add(gid);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse organizations claim.");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid token: malformed organizations claim.");
                return;
            }

            if (tenantIdsFromToken.Count == 0)
            {
                _logger.LogWarning("Organizations claim does not contain valid tenant IDs.");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid token: no valid organizations.");
                return;
            }

            Guid selectedTenantId;
            if (!context.Request.Headers.TryGetValue(TenantHeaderName, out var headerValues) || headerValues.Count == 0)
            {
                if (tenantIdsFromToken.Count == 1)
                {
                    selectedTenantId = tenantIdsFromToken[0];
                    _logger.LogDebug("No tenant header provided; implicitly selected the only available tenant {TenantId}.", selectedTenantId);
                }
                else
                {
                    _logger.LogWarning("Missing {Header} header and multiple organizations present.", TenantHeaderName);
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync($"Missing {TenantHeaderName} header.");
                    return;
                }
            }
            else
            {
                if (!Guid.TryParse(headerValues[0], out selectedTenantId))
                {
                    _logger.LogWarning("Invalid {Header} header value: {Value}", TenantHeaderName, headerValues[0]);
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync($"Invalid {TenantHeaderName} header.");
                    return;
                }
            }

            if (selectedTenantId == Guid.Empty && context.User.IsInRole(Roles.SystemAdmin))
            {
                _logger.LogInformation("Selected tenant ID is empty GUID and User has system administrator role. Assuming virtual master tenant.");

                tenantProvider.TenantId = Guid.Empty;
                tenantProvider.Name = "VIRTUAL MASTER";
                tenantProvider.CreatedAt = DateTimeOffset.MinValue;

                await _next(context);
            }

            if (!tenantIdsFromToken.Contains(selectedTenantId))
            {
                _logger.LogWarning("Selected tenant {TenantId} is not part of user's organizations.", selectedTenantId);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Forbidden: tenant not in token organizations.");
                return;
            }

            var cacheKey = $"{TenantCachePrefix}:{selectedTenantId}";
            if (!cache.TryGetValue(cacheKey, out Tenant? tenant))
            {
                tenant = await dbContext.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == selectedTenantId);

                if (tenant != null)
                {
                    cache.Set(cacheKey, tenant, TenantCacheExpiration);
                }
            }

            if (tenant == null)
            {
                _logger.LogWarning("Tenant not found or inactive: {TenantId}", selectedTenantId);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Tenant not found or inactive");
                return;
            }

            tenantProvider.TenantId = tenant.Id;
            tenantProvider.Name = tenant.Name;
            tenantProvider.Description = tenant.Description;
            tenantProvider.CreatedAt = tenant.CreatedAt;

            _logger.LogDebug("Resolved tenant {TenantId}", tenant.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving tenant");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("Error resolving tenant");
            return;
        }

        await _next(context);
    }
}
