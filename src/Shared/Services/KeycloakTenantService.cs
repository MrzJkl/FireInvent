using FireInvent.Shared.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FireInvent.Shared.Services;

/// <summary>
/// Service for managing Keycloak realms for tenant provisioning.
/// This service operates at the master realm level to create and manage tenant realms.
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
            ["username"] = _options.AdminUsername,
            ["password"] = _options.AdminPassword
        });

        var response = await _httpClient.PostAsync($"realms/master/protocol/openid-connect/token", tokenRequest);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(_jsonOptions);
        _accessToken = tokenResponse?.AccessToken 
            ?? throw new InvalidOperationException("Failed to obtain access token from Keycloak.");

        // Set token expiry with a buffer to avoid race conditions
        _tokenExpiry = DateTime.UtcNow.AddSeconds(
            (tokenResponse.ExpiresIn ?? DefaultTokenExpirySeconds) - TokenExpiryBufferSeconds);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    public async Task<bool> CreateTenantRealmAsync(string realmName, string displayName)
    {
        if (string.IsNullOrWhiteSpace(realmName))
            throw new ArgumentException("Realm name cannot be empty.", nameof(realmName));

        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));

        try
        {
            await EnsureAuthenticatedAsync();

            // Check if realm already exists
            if (await RealmExistsAsync(realmName))
            {
                _logger.LogWarning("Realm {RealmName} already exists", realmName);
                return false;
            }

            var realm = new KeycloakRealm
            {
                Realm = realmName,
                DisplayName = displayName,
                Enabled = true,
                // Default settings for tenant realms
                RegistrationAllowed = false,
                RegistrationEmailAsUsername = true,
                ResetPasswordAllowed = true,
                RememberMe = true,
                VerifyEmail = true,
                LoginWithEmailAllowed = true,
                DuplicateEmailsAllowed = false,
                SslRequired = "external",
                // Token settings
                AccessTokenLifespan = 300, // 5 minutes
                SsoSessionIdleTimeout = 1800, // 30 minutes
                SsoSessionMaxLifespan = 36000, // 10 hours
            };

            _logger.LogInformation("Creating realm: {RealmName}", realmName);

            var response = await _httpClient.PostAsJsonAsync("admin/realms", realm, _jsonOptions);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create realm {RealmName}: {StatusCode} - {Error}", 
                    realmName, response.StatusCode, errorContent);
                return false;
            }

            _logger.LogInformation("Successfully created realm: {RealmName}", realmName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating realm {RealmName}", realmName);
            return false;
        }
    }

    public async Task<bool> RealmExistsAsync(string realmName)
    {
        if (string.IsNullOrWhiteSpace(realmName))
            return false;

        try
        {
            await EnsureAuthenticatedAsync();

            var response = await _httpClient.GetAsync($"admin/realms/{Uri.EscapeDataString(realmName)}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if realm exists: {RealmName}", realmName);
            return false;
        }
    }

    public async Task<bool> DeleteTenantRealmAsync(string realmName)
    {
        if (string.IsNullOrWhiteSpace(realmName))
            throw new ArgumentException("Realm name cannot be empty.", nameof(realmName));

        try
        {
            await EnsureAuthenticatedAsync();

            _logger.LogInformation("Deleting realm: {RealmName}", realmName);

            var response = await _httpClient.DeleteAsync($"admin/realms/{Uri.EscapeDataString(realmName)}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to delete realm {RealmName}: {StatusCode} - {Error}", 
                    realmName, response.StatusCode, errorContent);
                return false;
            }

            _logger.LogInformation("Successfully deleted realm: {RealmName}", realmName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting realm {RealmName}", realmName);
            return false;
        }
    }

    public async Task ConfigureTenantRealmAsync(string realmName)
    {
        if (string.IsNullOrWhiteSpace(realmName))
            throw new ArgumentException("Realm name cannot be empty.", nameof(realmName));

        try
        {
            await EnsureAuthenticatedAsync();

            _logger.LogInformation("Configuring realm: {RealmName}", realmName);

            // Create required roles for the tenant realm
            await CreateRealmRolesAsync(realmName);

            _logger.LogInformation("Successfully configured realm: {RealmName}", realmName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring realm {RealmName}", realmName);
            throw new InvalidOperationException($"Failed to configure realm: {ex.Message}", ex);
        }
    }

    private async Task CreateRealmRolesAsync(string realmName)
    {
        var roles = new[]
        {
            new { Name = "admin", Description = "Administrator role with full access" },
            new { Name = "procurement", Description = "Procurement role for managing orders and inventory" },
            new { Name = "maintenance", Description = "Maintenance role for managing maintenance tasks" },
            new { Name = "integration", Description = "Integration role for API access" }
        };

        foreach (var role in roles)
        {
            try
            {
                var keycloakRole = new KeycloakRole
                {
                    Name = role.Name,
                    Description = role.Description
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"admin/realms/{Uri.EscapeDataString(realmName)}/roles",
                    keycloakRole,
                    _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Created role {RoleName} in realm {RealmName}", role.Name, realmName);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    _logger.LogDebug("Role {RoleName} already exists in realm {RealmName}", role.Name, realmName);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to create role {RoleName} in realm {RealmName}: {Error}", 
                        role.Name, realmName, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error creating role {RoleName} in realm {RealmName}", role.Name, realmName);
            }
        }
    }

    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }
    }

    private class KeycloakRealm
    {
        [JsonPropertyName("realm")]
        public string? Realm { get; set; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("enabled")]
        public bool? Enabled { get; set; }

        [JsonPropertyName("registrationAllowed")]
        public bool? RegistrationAllowed { get; set; }

        [JsonPropertyName("registrationEmailAsUsername")]
        public bool? RegistrationEmailAsUsername { get; set; }

        [JsonPropertyName("resetPasswordAllowed")]
        public bool? ResetPasswordAllowed { get; set; }

        [JsonPropertyName("rememberMe")]
        public bool? RememberMe { get; set; }

        [JsonPropertyName("verifyEmail")]
        public bool? VerifyEmail { get; set; }

        [JsonPropertyName("loginWithEmailAllowed")]
        public bool? LoginWithEmailAllowed { get; set; }

        [JsonPropertyName("duplicateEmailsAllowed")]
        public bool? DuplicateEmailsAllowed { get; set; }

        [JsonPropertyName("sslRequired")]
        public string? SslRequired { get; set; }

        [JsonPropertyName("accessTokenLifespan")]
        public int? AccessTokenLifespan { get; set; }

        [JsonPropertyName("ssoSessionIdleTimeout")]
        public int? SsoSessionIdleTimeout { get; set; }

        [JsonPropertyName("ssoSessionMaxLifespan")]
        public int? SsoSessionMaxLifespan { get; set; }
    }

    private class KeycloakRole
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
