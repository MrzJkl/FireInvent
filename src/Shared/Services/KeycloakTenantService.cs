using FireInvent.Contract.Extensions;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FireInvent.Shared.Services;

/// <summary>
/// Service for managing Keycloak Organizations for tenant provisioning.
/// All users live in a single realm; tenants are represented by Organizations.
/// </summary>
public class KeycloakTenantService : IKeycloakTenantService
{
    private const int TokenExpiryBufferSeconds = 30;
    private const int DefaultTokenExpirySeconds = 300;

    private readonly HttpClient _httpClient;
    private readonly KeycloakAdminOptions _options;
    private readonly ILogger<KeycloakTenantService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public KeycloakTenantService(
        HttpClient httpClient,
        IOptions<KeycloakAdminOptions> options,
        ILogger<KeycloakTenantService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.Url))
            throw new InvalidOperationException("Keycloak URL is not configured.");

        if (string.IsNullOrWhiteSpace(_options.AdminUsername))
            throw new InvalidOperationException("Keycloak admin username is not configured.");

        if (string.IsNullOrWhiteSpace(_options.AdminPassword))
            throw new InvalidOperationException("Keycloak admin password is not configured.");

        if (string.IsNullOrWhiteSpace(_options.Realm))
            throw new InvalidOperationException("Keycloak realm is not configured.");

        _httpClient.BaseAddress = new Uri(_options.Url.TrimEnd('/') + "/");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    private async Task EnsureAuthenticatedAsync()
    {
        if (_accessToken != null && DateTime.UtcNow < _tokenExpiry)
            return;

        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "admin-cli",
            ["username"] = _options.AdminUsername!,
            ["password"] = _options.AdminPassword!
        });

        var response = await _httpClient.PostAsync($"realms/master/protocol/openid-connect/token", tokenRequest);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(_jsonOptions);
        _accessToken = tokenResponse?.AccessToken
            ?? throw new InvalidOperationException("Failed to obtain access token from Keycloak.");

        _tokenExpiry = DateTime.UtcNow.AddSeconds(
            (tokenResponse.ExpiresIn ?? DefaultTokenExpirySeconds) - TokenExpiryBufferSeconds);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    /// <summary>
    /// Creates a tenant Organization with a name and description. Returns the GUID of the created organization.
    /// </summary>
    public async Task<Guid> CreateTenantOrganizationAsync(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Organization name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        var alias = SanitizeNameForKeycloak(name);

        try
        {
            await EnsureAuthenticatedAsync();

            // Optional: Check if an organization with the same normalized name already exists
            var listResponse = await _httpClient.GetAsync($"admin/realms/{Uri.EscapeDataString(_options.Realm)}/organizations");
            if (!listResponse.IsSuccessStatusCode)
            {
                var error = await listResponse.Content.ReadAsStringAsync();
                _logger.LogError("Failed to list organizations: {StatusCode} - {Error}", listResponse.StatusCode, error);
                throw new KeycloakException();
            }

            var existingOrgs = await listResponse.Content.ReadFromJsonAsync<List<JsonElement>>(_jsonOptions) ?? new List<JsonElement>();
            var organizationExists = existingOrgs.Any(org =>
            {
                if (org.TryGetProperty("name", out var orgNameProp))
                {
                    var orgName = orgNameProp.GetString();
                    return orgName == name;
                }

                if (org.TryGetProperty("alias", out var orgAliasProp))
                {
                    var orgAlias = orgAliasProp.GetString();
                    return orgAlias == alias;
                }

                return false;
            });
            if (organizationExists)
            {
                _logger.LogWarning("Organization with name '{Name}' already exists.", name.SanitizeForLogging());
                throw new KeycloakException();
            }

            var payload = new
            {
                name,
                alias,
                description,
            };

            var createResponse = await _httpClient.PostAsJsonAsync(
                $"admin/realms/{Uri.EscapeDataString(_options.Realm)}/organizations",
                payload,
                _jsonOptions);

            if (createResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("Created organization '{Name}'", name.SanitizeForLogging());

                Uri? location = createResponse.Headers.Location;
                if (location == null)
                {
                    if (createResponse.Headers.TryGetValues("Location", out var values))
                    {
                        var first = values.FirstOrDefault();
                        if (!string.IsNullOrEmpty(first))
                        {
                            location = Uri.TryCreate(first, UriKind.RelativeOrAbsolute, out var tmp) ? tmp : null;
                        }
                    }
                }

                if (location == null)
                {
                    _logger.LogError("Create organization response missing Location header.");
                    throw new KeycloakException();
                }

                // Return http://localhost:8080/admin/realms/fireinvent/organizations/11261bc8-4dd7-43a7-a1dd-c683f80d7838 in Location Header
                var lastSegment = location.Segments.LastOrDefault()?.Trim('/');
                if (!string.IsNullOrWhiteSpace(lastSegment) && Guid.TryParse(lastSegment, out var idFromLocation))
                {
                    return idFromLocation;
                }

                _logger.LogError("Created organization resource did not contain a valid 'id'.");
                throw new KeycloakException();
            }

            var createError = await createResponse.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create organization '{Name}': {StatusCode} - {Error}", name.SanitizeForLogging(), createResponse.StatusCode, createError);
            throw new KeycloakException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating organization {Name}", name.SanitizeForLogging());
            throw new KeycloakException();
        }
    }

    public async Task UpdateTenantOrganizationNameAsync(Guid organizationId, string newName, string? newDescription)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization ID cannot be empty.", nameof(organizationId));
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("New name cannot be empty.", nameof(newName));

        var alias = SanitizeNameForKeycloak(newName);

        try
        {
            await EnsureAuthenticatedAsync();

            var payload = new
            {
                name = newName,
                description = newDescription,
                alias
            };

            var response = await _httpClient.PutAsJsonAsync(
                $"admin/realms/{Uri.EscapeDataString(_options.Realm)}/organizations/{organizationId}",
                payload,
                _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to update organization name '{Name}' ({Id}): {StatusCode} - {Error}", newName.SanitizeForLogging(), organizationId, response.StatusCode, error);
                throw new KeycloakException();
            }

            _logger.LogInformation("Updated organization ({Id}) name to '{Name}'", organizationId, newName.SanitizeForLogging());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organization name ({Id})", organizationId);
            throw new KeycloakException();
        }
}
    private static string SanitizeNameForKeycloak(string name)
    {
        var sanitized = new string(name
            .Trim()
            .ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray());

        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, "-+", "-");

        return sanitized.Trim('-');
    }

    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }
    }
}
