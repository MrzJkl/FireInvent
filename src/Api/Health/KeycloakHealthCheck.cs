using System.Text.Json;
using FireInvent.Shared.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace FireInvent.Api.Health;

public class KeycloakHealthCheck(
    IHttpClientFactory httpClientFactory,
    IOptions<KeycloakAdminOptions> options,
    ILogger<KeycloakHealthCheck> logger)
    : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var opts = options.Value;
        if (string.IsNullOrWhiteSpace(opts.Url) || string.IsNullOrWhiteSpace(opts.AdminClientId))
        {
            return HealthCheckResult.Unhealthy("KeycloakAdminOptions are not configured (Url/AdminClientId).");
        }

        var client = httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(5);

        // Reachability
        var wellKnownUrl = $"{opts.Url.TrimEnd('/')}/realms/{opts.Realm}/.well-known/openid-configuration";
        try
        {
            using var resp = await client.GetAsync(wellKnownUrl, cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                return HealthCheckResult.Unhealthy("Keycloak not reachable or returned non-success for well-known endpoint.", data: new Dictionary<string, object>
                {
                    {"statusCode", (int)resp.StatusCode},
                    {"endpoint", wellKnownUrl}
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Keycloak reachability check failed.");
            return HealthCheckResult.Unhealthy("Keycloak not reachable.", ex);
        }

        // Authentication
        var tokenUrl = $"{opts.Url.TrimEnd('/')}/realms/{opts.Realm}/protocol/openid-connect/token";
        using var body = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", opts.AdminClientId),
            new KeyValuePair<string, string>("client_secret", opts.AdminClientSecret)
        });

        try
        {
            using var tokenResp = await client.PostAsync(tokenUrl, body, cancellationToken);
            if (!tokenResp.IsSuccessStatusCode)
            {
                return HealthCheckResult.Unhealthy("Keycloak authentication failed.", data: new Dictionary<string, object>
                {
                    {"statusCode", (int)tokenResp.StatusCode},
                    {"endpoint", tokenUrl}
                });
            }

            var json = await tokenResp.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            if (!json.TryGetProperty("access_token", out _))
            {
                return HealthCheckResult.Unhealthy("Keycloak authentication did not return an access_token.");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Keycloak authentication check failed.");
            return HealthCheckResult.Unhealthy("Keycloak authentication failed.", ex);
        }

        return HealthCheckResult.Healthy("Keycloak reachable and authentication succeeded.");
    }
}
