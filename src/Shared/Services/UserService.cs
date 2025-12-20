using FireInvent.Contract;
using FireInvent.Shared.Models;
using FireInvent.Shared.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FireInvent.Shared.Services;

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserService> _logger;
    private readonly UserContextProvider _tenantProvider;
    private readonly KeycloakAdminOptions _options;
    
    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    
    private const int DefaultTokenExpirySeconds = 300;
    private const int TokenExpiryBufferSeconds = 30;
    
    private readonly JsonSerializerOptions _jsonOptions;

    public UserService(
        HttpClient httpClient,
        ILogger<UserService> logger,
        UserContextProvider userContextProvider,
        IOptions<KeycloakAdminOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _tenantProvider = userContextProvider;
        _options = options.Value;

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
            ["username"] = _options.AdminUsername,
            ["password"] = _options.AdminPassword
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

    public async Task<UserModel?> GetUserByIdAsync(Guid id)
    {
        if (!_tenantProvider.TenantId.HasValue)
        {
            _logger.LogWarning("Tenant ID is not set in TenantProvider. Cannot retrieve user.");
            return null;
        }

        try
        {
            await EnsureAuthenticatedAsync();

            var response = await _httpClient.GetAsync(
                $"admin/realms/{Uri.EscapeDataString(_options.Realm)}/users/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogDebug("User with ID {UserId} not found in Keycloak.", id);
                    return null;
                }
                
                response.EnsureSuccessStatusCode();
            }

            var keycloakUser = await response.Content.ReadFromJsonAsync<KeycloakUser>(_jsonOptions);
            
            if (keycloakUser == null)
                return null;

            // Verify user belongs to current tenant organization
            if (!await IsUserMemberOfCurrentOrganizationAsync(id.ToString()))
            {
                _logger.LogDebug("User {UserId} is not a member of current organization {TenantId}.", id, _tenantProvider.TenantId);
                return null;
            }

            return MapKeycloakUserToUserModel(keycloakUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user with ID {UserId} from Keycloak.", id);
            throw;
        }
    }

    public async Task<List<UserModel>> GetAllUsersAsync()
    {
        if (!_tenantProvider.TenantId.HasValue)
        {
            _logger.LogWarning("Tenant ID is not set in TenantProvider. Cannot retrieve users.");
            return [];
        }

        try
        {
            await EnsureAuthenticatedAsync();

            // Get all members of the current organization
            var response = await _httpClient.GetAsync(
                $"admin/realms/{Uri.EscapeDataString(_options.Realm)}/organizations/{_tenantProvider.TenantId}/members");
            
            response.EnsureSuccessStatusCode();

            var keycloakUsers = await response.Content.ReadFromJsonAsync<List<KeycloakUser>>(_jsonOptions)
                ?? new List<KeycloakUser>();

            var userModels = keycloakUsers
                .Select(MapKeycloakUserToUserModel)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToList();

            _logger.LogInformation("Retrieved {Count} users from Keycloak for tenant {TenantId}.", userModels.Count, _tenantProvider.TenantId);

            return userModels;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve users from Keycloak for tenant {TenantId}.", _tenantProvider.TenantId);
            throw;
        }
    }

    private async Task<bool> IsUserMemberOfCurrentOrganizationAsync(string userId)
    {
        if (!_tenantProvider.TenantId.HasValue)
            return false;

        try
        {
            var response = await _httpClient.GetAsync(
                $"admin/realms/{Uri.EscapeDataString(_options.Realm)}/organizations/{_tenantProvider.TenantId}/members/{userId}");
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if user {UserId} is member of organization {TenantId}.", userId, _tenantProvider.TenantId);
            return false;
        }
    }

    private static UserModel MapKeycloakUserToUserModel(KeycloakUser keycloakUser)
    {
        return new UserModel
        {
            Id = Guid.Parse(keycloakUser.Id ?? throw new InvalidOperationException("User ID is missing.")),
            FirstName = (keycloakUser.FirstName is null && keycloakUser.LastName is null) ?  keycloakUser.Username : keycloakUser.FirstName ?? string.Empty,
            LastName = (keycloakUser.FirstName is null && keycloakUser.LastName is null) ? keycloakUser.Username : keycloakUser.LastName ?? string.Empty,
            EMail = keycloakUser.Email ?? string.Empty,
        };
    }

    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }
    }

    private class KeycloakUser
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("enabled")]
        public bool? Enabled { get; set; }

        [JsonPropertyName("emailVerified")]
        public bool? EmailVerified { get; set; }
    }
}
