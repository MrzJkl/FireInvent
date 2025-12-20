using FireInvent.Contract;
using FireInvent.Database;
using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using System.Text.Json;

namespace FireInvent.Api.Middlewares;

/// <summary>
/// Middleware that resolves the active tenant and user per request based on Keycloak Organizations.
/// The JWT contains a list of organizations the user belongs to; the client must specify the active tenant via header.
/// </summary>
public class UserContextResolutionMiddleware(
    RequestDelegate next,
    ILogger<UserContextResolutionMiddleware> logger)
{
    private readonly ILogger<UserContextResolutionMiddleware> _logger = logger;
    private const string TenantCachePrefix = "tenant_by_id";
    private static readonly TimeSpan TenantCacheExpiration = TimeSpan.FromMinutes(15);
    private const string TenantHeaderName = "X-Tenant-Id";
    private const string OrganizationsClaimName = "organization"; // per provided token

    public async Task InvokeAsync(
        HttpContext context,
        UserContextProvider userContextProvider,
        AppDbContext dbContext,
        IMemoryCache cache)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await next(context);
            return;
        }

        try
        {
            // Extract UserId from the JWT token (sub claim)
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier) 
                ?? context.User.FindFirst("sub");
            
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                userContextProvider.UserId = userId;
                _logger.LogDebug("Resolved user ID {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("Unable to extract user ID from token claims.");
            }

            // Early check: allow virtual master selection via header when user is system admin
            if (context.Request.Headers.TryGetValue(TenantHeaderName, out var headerVals) && headerVals.Count > 0)
            {
                if (Guid.TryParse(headerVals[0], out var headerGuid) && headerGuid == Guid.Empty)
                {
                    if (context.User.IsInRole(Roles.SystemAdmin))
                    {
                        _logger.LogInformation("Selected tenant ID is empty GUID and user has system administrator role. Using virtual master tenant.");
                        userContextProvider.TenantId = Guid.Empty;
                        userContextProvider.Name = "VIRTUAL MASTER";
                        userContextProvider.CreatedAt = DateTimeOffset.MinValue;
                        context.Response.Headers["X-Resolved-Tenant-Id"] = Guid.Empty.ToString();
                        await next(context);
                        return;
                    }
                    else
                    {
                        _logger.LogWarning("Empty {Header} provided but user is not system admin.", TenantHeaderName);
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsync("Forbidden: master tenant requires system administrator role.");
                        return;
                    }
                }
            }

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
                    // Sanitize header value before logging to prevent log injection
                    var sanitizedValue = SanitizeForLogging(headerValues[0]);
                    _logger.LogWarning("Invalid {Header} header value: {Value}", TenantHeaderName, sanitizedValue);
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync($"Invalid {TenantHeaderName} header.");
                    return;
                }
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

            userContextProvider.TenantId = tenant.Id;
            userContextProvider.Name = tenant.Name;
            userContextProvider.Description = tenant.Description;
            userContextProvider.CreatedAt = tenant.CreatedAt;

            context.Response.Headers["X-Resolved-Tenant-Id"] = tenant.Id.ToString();

            _logger.LogDebug("Resolved tenant {TenantId} ({TenantName})", tenant.Id, tenant.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving tenant");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("Error resolving tenant");
            return;
        }

        await next(context);
    }

    /// <summary>
    /// Sanitizes a string for safe logging by removing newlines and control characters
    /// to prevent log injection attacks.
    /// </summary>
    private static string SanitizeForLogging(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Remove newlines, carriage returns, and other control characters
        return System.Text.RegularExpressions.Regex.Replace(input, @"[\r\n\t\x00-\x1F\x7F]", "_");
    }
}
