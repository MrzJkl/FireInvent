using FireInvent.Contract;
using FireInvent.Database;
using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;

namespace FireInvent.Api.Middlewares;

/// <summary>
/// Middleware that resolves the tenant for each request by extracting the realm
/// from the JWT token and looking up the corresponding tenant in the database.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;
    private const string TenantCachePrefix = "tenant_by_realm:";
    private static readonly TimeSpan TenantCacheExpiration = TimeSpan.FromMinutes(15);

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
        // Skip tenant resolution for anonymous endpoints
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        try
        {
            // Extract realm from JWT token claims
            var issuerClaim = context.User.Claims
                .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iss);

            if (issuerClaim == null)
            {
                _logger.LogWarning("No issuer claim found in JWT token. Cannot extract tenant information!");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid token: missing issuer.");
                return;
            }

            // Extract realm from issuer URL (e.g., "https://keycloak.example.com/realms/fire-dept-berlin" -> "fire-dept-berlin")
            string? realm = null;
            try
            {
                var issuerUri = new Uri(issuerClaim.Value);
                var pathSegments = issuerUri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                
                // Look for "realms" segment and get the next one
                for (int i = 0; i < pathSegments.Length - 1; i++)
                {
                    if (pathSegments[i].Equals("realms", StringComparison.OrdinalIgnoreCase))
                    {
                        realm = pathSegments[i + 1];
                        break;
                    }
                }
            }
            catch (UriFormatException ex)
            {
                _logger.LogWarning(ex, "Invalid URI format in issuer claim: {Issuer}", issuerClaim.Value);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid token: malformed issuer");
                return;
            }

            if (string.IsNullOrEmpty(realm))
            {
                _logger.LogWarning("Could not extract realm from issuer: {Issuer}", issuerClaim.Value);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid token: could not determine tenant");
                return;
            }

            // Try to get tenant from cache first
            var cacheKey = $"{TenantCachePrefix}{realm}";

            if (!cache.TryGetValue(cacheKey, out Tenant? tenant))
            {
                tenant = await dbContext.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Realm == realm);

                if (tenant != null)
                {
                    cache.Set(cacheKey, tenant, TenantCacheExpiration);
                }
            }

            if (tenant == null)
            {
                _logger.LogWarning("Tenant not found for realm: {Realm}", realm);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Tenant not found or inactive");
                return;
            }

            tenantProvider.TenantId = tenant.Id;
            
            _logger.LogDebug("Resolved tenant {TenantId} for realm {Realm}", tenant.Id, realm);
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
