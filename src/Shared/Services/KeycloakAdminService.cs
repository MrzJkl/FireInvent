using FireInvent.Contract;
using FireInvent.Contract.Extensions;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FireInvent.Shared.Services;

/// <summary>
/// Service for managing API Integrations within tenants using Keycloak Organizations.
/// </summary>
public class KeycloakAdminService : IKeycloakAdminService
{
    private const int TokenExpiryBufferSeconds = 30;
    private const int DefaultTokenExpirySeconds = 300;
    private const int IntegrationTokenLifespanSeconds = 3600;

    private readonly HttpClient _httpClient;
    private readonly KeycloakAdminOptions _options;
    private readonly ILogger<KeycloakAdminService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private UserContextProvider _tenantProvider;

    public KeycloakAdminService(
        HttpClient httpClient,
        IOptions<KeycloakAdminOptions> options,
        ILogger<KeycloakAdminService> logger,
        UserContextProvider userContextProvider)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _tenantProvider = userContextProvider;

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

        // Set token expiry with a buffer to avoid race conditions
        _tokenExpiry = DateTime.UtcNow.AddSeconds(
            (tokenResponse.ExpiresIn ?? DefaultTokenExpirySeconds) - TokenExpiryBufferSeconds);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
    }

    public async Task<ApiIntegrationCredentialsModel> CreateApiIntegrationAsync(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        var clientId = $"{_options.ApiClientPrefix}-{_tenantProvider.TenantId}-{SanitizeClientId(name)}";

        await EnsureAuthenticatedAsync();

        var existingClients = await GetClientsByClientIdAsync(clientId);
        if (existingClients.Any())
        {
            _logger.LogWarning("Attempted to create API integration with duplicate client ID: {ClientId}", clientId.SanitizeForLogging());
            throw new ConflictException($"An API integration with the name '{name}' already exists.");
        }

        var client = new KeycloakClient
        {
            ClientId = clientId,
            Name = name,
            Enabled = true,
            ClientAuthenticatorType = "client-secret",
            PublicClient = false,
            ServiceAccountsEnabled = true,
            StandardFlowEnabled = false,
            ImplicitFlowEnabled = false,
            DirectAccessGrantsEnabled = false,
            Attributes = new Dictionary<string, string>
            {
                ["description"] = description ?? string.Empty,
                ["access.token.lifespan"] = IntegrationTokenLifespanSeconds.ToString()
            },
            ProtocolMappers = new List<ProtocolMapper>
            {
                // Add protocol mapper to include realm roles in the token
                new ProtocolMapper
                {
                    Name = "realm roles",
                    Protocol = "openid-connect",
                    ProtocolMapperType = "oidc-usermodel-realm-role-mapper",
                    Config = new Dictionary<string, string>
                    {
                        ["claim.name"] = "roles",
                        ["jsonType.label"] = "String",
                        ["multivalued"] = "true",
                        ["userinfo.token.claim"] = "true",
                        ["id.token.claim"] = "true",
                        ["access.token.claim"] = "true"
                    }
                }
            }
        };

        try
        {
            _logger.LogInformation("Creating API integration in realm {Realm} with client ID: {ClientId}", _options.Realm, clientId.SanitizeForLogging());

            var response = await _httpClient.PostAsJsonAsync(
                $"admin/realms/{_options.Realm}/clients", 
                client, 
                _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to create client: {response.StatusCode} - {errorContent}");
            }

            var createdClients = await GetClientsByClientIdAsync(clientId);
            var createdClient = createdClients.FirstOrDefault()
                ?? throw new InvalidOperationException("Failed to retrieve the created client.");

            await AssignIntegrationRoleToServiceAccountAsync(createdClient.Id!);

            // Add membership of the service account user to the current organization (tenant)
            var serviceAccountResponse = await _httpClient.GetAsync(
                $"admin/realms/{_options.Realm}/clients/{createdClient.Id}/service-account-user");
            serviceAccountResponse.EnsureSuccessStatusCode();
            var serviceAccountUser = await serviceAccountResponse.Content.ReadFromJsonAsync<ServiceAccountUser>(_jsonOptions);
            var serviceAccountUserId = serviceAccountUser?.Id
                ?? throw new InvalidOperationException("Failed to retrieve service account user ID.");

            await AddUserToOrganizationAsync(_tenantProvider.TenantId.Value, serviceAccountUserId);

            var secretResponse = await _httpClient.GetAsync(
                $"admin/realms/{_options.Realm}/clients/{createdClient.Id}/client-secret");
            secretResponse.EnsureSuccessStatusCode();

            var credentials = await secretResponse.Content.ReadFromJsonAsync<ClientSecretResponse>(_jsonOptions);
            var clientSecret = credentials?.Value
                ?? throw new InvalidOperationException("Failed to retrieve the client secret.");

            _logger.LogInformation("Successfully created API integration: {ClientId}", clientId.SanitizeForLogging());

            return new ApiIntegrationCredentialsModel
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                Name = name,
            };
        }
        catch (Exception ex) when (ex is not ConflictException)
        {
            _logger.LogError(ex, "Failed to create API integration with client ID: {ClientId}", clientId.SanitizeForLogging());
            throw new InvalidOperationException($"Failed to create API integration: {ex.Message}", ex);
        }
    }

    public async Task<List<ApiIntegrationModel>> GetApiIntegrationsAsync()
    {
        try
        {
            await EnsureAuthenticatedAsync();

            _logger.LogDebug("Fetching all clients from Keycloak realm: {Realm}", _options.Realm);

            var response = await _httpClient.GetAsync($"admin/realms/{_options.Realm}/clients");
            response.EnsureSuccessStatusCode();

            var clients = await response.Content.ReadFromJsonAsync<List<KeycloakClient>>(_jsonOptions)
                ?? new List<KeycloakClient>();

            // Filter clients to only those belonging to current tenant organization
            var filtered = new List<KeycloakClient>();
            foreach (var c in clients.Where(c => c.ClientId?.StartsWith(_options.ApiClientPrefix) == true))
            {
                if (string.IsNullOrEmpty(c.Id)) continue;
                if (await IsClientServiceAccountMemberOfCurrentOrganizationAsync(c.Id))
                {
                    filtered.Add(c);
                }
            }

            var apiIntegrations = filtered
                .Select(c => new ApiIntegrationModel
                {
                    ClientId = c.ClientId!,
                    Name = c.Name ?? ExtractNameFromClientId(c.ClientId!),
                    Description = c.Attributes?.ContainsKey("description") == true 
                        ? c.Attributes["description"] 
                        : null,
                    Enabled = c.Enabled ?? false,
                })
                .OrderBy(i => i.Name)
                .ToList();

            _logger.LogInformation("Found {Count} API integrations in tenant {TenantId}", apiIntegrations.Count, _tenantProvider.TenantId);

            return apiIntegrations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve API integrations from Keycloak");
            throw new InvalidOperationException("Failed to retrieve API integrations from Keycloak.", ex);
        }
    }

    public async Task DeleteApiIntegrationAsync(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Client ID cannot be empty.", nameof(clientId));

        if (!clientId.StartsWith(_options.ApiClientPrefix))
            throw new ArgumentException($"Client ID must start with '{_options.ApiClientPrefix}'.", nameof(clientId));

        try
        {
            await EnsureAuthenticatedAsync();

            // Find the client by client ID to get its internal ID
            var clients = await GetClientsByClientIdAsync(clientId);
            var client = clients.FirstOrDefault();

            if (client == null)
            {
                _logger.LogWarning("Attempted to delete non-existent API integration: {ClientId}", clientId.SanitizeForLogging());
                throw new NotFoundException($"API integration with client ID '{clientId}' not found.");
            }

            // Ensure the client belongs to the current tenant organization
            if (!await IsClientServiceAccountMemberOfCurrentOrganizationAsync(client.Id!))
            {
                _logger.LogWarning("Attempted to delete API integration outside of tenant {TenantId}: {ClientId}", _tenantProvider.TenantId, clientId.SanitizeForLogging());
                throw new InvalidOperationException("Forbidden: client does not belong to current tenant.");
            }

            _logger.LogInformation("Deleting API integration: {ClientId} (internal ID: {InternalId})", clientId.SanitizeForLogging(), client.Id);
            
            var response = await _httpClient.DeleteAsync($"admin/realms/{_options.Realm}/clients/{client.Id}");
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully deleted API integration: {ClientId}", clientId.SanitizeForLogging());
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
            _logger.LogError(ex, "Failed to delete API integration: {ClientId}", clientId.SanitizeForLogging());
            throw new InvalidOperationException($"Failed to delete API integration: {ex.Message}", ex);
        }
    }

    public async Task<bool> ApiIntegrationExistsAsync(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            return false;

        try
        {
            await EnsureAuthenticatedAsync();
            var clients = await GetClientsByClientIdAsync(clientId);
            return clients.Any(c => c.ClientId == clientId && c.ClientId.StartsWith(_options.ApiClientPrefix));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if API integration exists: {ClientId}", clientId.SanitizeForLogging());
            return false;
        }
    }

    private async Task AssignIntegrationRoleToServiceAccountAsync(string clientUuid)
    {
        try
        {
            var serviceAccountResponse = await _httpClient.GetAsync(
                $"admin/realms/{_options.Realm}/clients/{clientUuid}/service-account-user");
            serviceAccountResponse.EnsureSuccessStatusCode();

            var serviceAccountUser = await serviceAccountResponse.Content.ReadFromJsonAsync<ServiceAccountUser>(_jsonOptions);
            var serviceAccountUserId = serviceAccountUser?.Id
                ?? throw new InvalidOperationException("Failed to retrieve service account user ID.");

            var rolesResponse = await _httpClient.GetAsync(
                $"admin/realms/{_options.Realm}/roles/integration");
            rolesResponse.EnsureSuccessStatusCode();

            var integrationRole = await rolesResponse.Content.ReadFromJsonAsync<KeycloakRole>(_jsonOptions);
            if (integrationRole == null)
                throw new InvalidOperationException("Integration role not found in Keycloak.");

            var roleMapping = new List<KeycloakRole> { integrationRole };
            var assignRoleResponse = await _httpClient.PostAsJsonAsync(
                $"admin/realms/{_options.Realm}/users/{serviceAccountUserId}/role-mappings/realm",
                roleMapping,
                _jsonOptions);
            assignRoleResponse.EnsureSuccessStatusCode();

            _logger.LogInformation("Assigned integration role to service account for client: {ClientUuid}", clientUuid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assign integration role to service account for client: {ClientUuid}", clientUuid);
            throw new InvalidOperationException("Failed to assign integration role to service account.", ex);
        }
    }

    private async Task AddUserToOrganizationAsync(Guid organizationId, string userId)
    {
        var url = $"admin/realms/{_options.Realm}/organizations/{organizationId}/members";
        var json = JsonSerializer.Serialize(userId);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                _logger.LogDebug("User {UserId} is already a member of organization {OrgId}", userId, organizationId);
                return;
            }
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to add user {UserId} to organization {OrgId}: {Status} - {Error}", userId, organizationId, response.StatusCode, error);
            throw new InvalidOperationException("Failed to add service account to organization.");
        }
        _logger.LogInformation("Added user {UserId} to organization {OrgId}", userId, organizationId);
    }

    private async Task<bool> IsClientServiceAccountMemberOfCurrentOrganizationAsync(string clientUuid)
    {
        var serviceAccountResponse = await _httpClient.GetAsync(
            $"admin/realms/{_options.Realm}/clients/{clientUuid}/service-account-user");
        serviceAccountResponse.EnsureSuccessStatusCode();
        var serviceAccountUser = await serviceAccountResponse.Content.ReadFromJsonAsync<ServiceAccountUser>(_jsonOptions);
        var userId = serviceAccountUser?.Id;
        if (string.IsNullOrEmpty(userId)) return false;

        var orgId = _tenantProvider.TenantId;
        var membersResponse = await _httpClient.GetAsync(
            $"admin/realms/{_options.Realm}/organizations/{orgId}/members?userId={Uri.EscapeDataString(userId)}");
        if (!membersResponse.IsSuccessStatusCode)
        {
            return false;
        }

        try
        {
            var json = await membersResponse.Content.ReadFromJsonAsync<List<JsonElement>>(_jsonOptions);
            if (json != null && json.Any(m => m.TryGetProperty("id", out var idProp) && idProp.GetString() == userId))
                return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if service account is member of current organization.");
            return false;
        }
        return false;
    }

    private async Task<List<KeycloakClient>> GetClientsByClientIdAsync(string clientId)
    {
        var response = await _httpClient.GetAsync(
            $"admin/realms/{_options.Realm}/clients?clientId={Uri.EscapeDataString(clientId)}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<KeycloakClient>>(_jsonOptions)
            ?? new List<KeycloakClient>();
    }

    private static string SanitizeClientId(string name)
    {
        var sanitized = new string(name
            .Trim()
            .ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray());

        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, "-+", "-");

        return sanitized.Trim('-');
    }

    private string ExtractNameFromClientId(string clientId)
    {
        if (!clientId.StartsWith(_options.ApiClientPrefix))
            return clientId;

        return clientId[_options.ApiClientPrefix.Length..]
            .Replace('-', ' ')
            .Trim();
    }

    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }
    }

    private class KeycloakClient
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("clientId")]
        public string? ClientId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("enabled")]
        public bool? Enabled { get; set; }

        [JsonPropertyName("clientAuthenticatorType")]
        public string? ClientAuthenticatorType { get; set; }

        [JsonPropertyName("publicClient")]
        public bool? PublicClient { get; set; }

        [JsonPropertyName("serviceAccountsEnabled")]
        public bool? ServiceAccountsEnabled { get; set; }

        [JsonPropertyName("standardFlowEnabled")]
        public bool? StandardFlowEnabled { get; set; }

        [JsonPropertyName("implicitFlowEnabled")]
        public bool? ImplicitFlowEnabled { get; set; }

        [JsonPropertyName("directAccessGrantsEnabled")]
        public bool? DirectAccessGrantsEnabled { get; set; }

        [JsonPropertyName("attributes")]
        public Dictionary<string, string>? Attributes { get; set; }

        [JsonPropertyName("protocolMappers")]
        public List<ProtocolMapper>? ProtocolMappers { get; set; }
    }

    private class ProtocolMapper
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("protocol")]
        public string? Protocol { get; set; }

        [JsonPropertyName("protocolMapper")]
        public string? ProtocolMapperType { get; set; }

        [JsonPropertyName("config")]
        public Dictionary<string, string>? Config { get; set; }
    }

    private class ClientSecretResponse
    {
        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }

    private class ServiceAccountUser
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }
    }

    private class KeycloakRole
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
