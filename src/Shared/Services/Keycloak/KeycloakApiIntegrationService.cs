using FireInvent.Contract;
using FireInvent.Contract.Extensions;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FireInvent.Shared.Services.Keycloak;

/// <summary>
/// Service for managing API Integrations within tenants using Keycloak Organizations.
/// </summary>
public class KeycloakApiIntegrationService(
    KeycloakHttpClient keycloakClient,
    IOptions<KeycloakAdminOptions> options,
    ILogger<KeycloakApiIntegrationService> logger,
    UserContextProvider userContextProvider) : IKeycloakApiIntegrationService
{
    private const int IntegrationTokenLifespanSeconds = 3600;
    private readonly KeycloakAdminOptions _options = options.Value;

    public async Task<ApiIntegrationCredentialsModel> CreateApiIntegrationAsync(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        var clientId = $"{_options.ApiClientPrefix}-{userContextProvider.TenantId}-{SanitizeClientId(name)}";

        var existingClients = await GetClientsByClientIdAsync(clientId);
        if (existingClients.Any())
        {
            logger.LogWarning("Client with ID {ClientId} already exists.", clientId.SanitizeForLogging());
            throw new ConflictException($"API integration with name '{name}' already exists.");
        }

        try
        {
            var client = new
            {
                clientId,
                name,
                enabled = true,
                clientAuthenticatorType = "client-secret",
                publicClient = false,
                serviceAccountsEnabled = true,
                standardFlowEnabled = false,
                implicitFlowEnabled = false,
                directAccessGrantsEnabled = false,
                attributes = new Dictionary<string, string>
                {
                    ["description"] = description ?? string.Empty,
                    ["access.token.lifespan"] = IntegrationTokenLifespanSeconds.ToString()
                },
                protocolMappers = new[]
                {
                    new
                    {
                        name = "tenant_id",
                        protocol = "openid-connect",
                        protocolMapper = "oidc-hardcoded-claim-mapper",
                        config = new Dictionary<string, string>
                        {
                            ["claim.name"] = "tenant_id",
                            ["claim.value"] = userContextProvider.TenantId.ToString() ?? string.Empty,
                            ["jsonType.label"] = "String",
                            ["id.token.claim"] = "true",
                            ["access.token.claim"] = "true",
                            ["userinfo.token.claim"] = "true"
                        }
                    }
                }
            };

            logger.LogInformation("Creating API integration in realm {Realm} with client ID: {ClientId}", keycloakClient.Realm, clientId.SanitizeForLogging());

            var response = await keycloakClient.PostAsJsonAsync(
                $"admin/realms/{keycloakClient.Realm}/clients", 
                client);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to create client: {response.StatusCode} - {errorContent}");
            }

            var createdClients = await GetClientsByClientIdAsync(clientId);
            var createdClient = createdClients.FirstOrDefault()
                ?? throw new InvalidOperationException("Failed to retrieve the created client.");

            await AssignIntegrationRoleToServiceAccountAsync(createdClient.Id!);

            var serviceAccountResponse = await keycloakClient.GetAsync(
                $"admin/realms/{keycloakClient.Realm}/clients/{createdClient.Id}/service-account-user");
            serviceAccountResponse.EnsureSuccessStatusCode();
            var serviceAccountUser = await serviceAccountResponse.Content.ReadFromJsonAsync<KeycloakServiceAccountUser>(keycloakClient.JsonOptions);
            var serviceAccountUserId = serviceAccountUser?.Id
                ?? throw new InvalidOperationException("Failed to retrieve service account user ID.");

            await AddUserToOrganizationAsync(userContextProvider.TenantId.Value, serviceAccountUserId);

            var secretResponse = await keycloakClient.GetAsync(
                $"admin/realms/{keycloakClient.Realm}/clients/{createdClient.Id}/client-secret");
            secretResponse.EnsureSuccessStatusCode();

            var credentials = await secretResponse.Content.ReadFromJsonAsync<KeycloakClientSecretResponse>(keycloakClient.JsonOptions);
            var clientSecret = credentials?.Value
                ?? throw new InvalidOperationException("Failed to retrieve the client secret.");

            logger.LogInformation("Successfully created API integration: {ClientId}", clientId.SanitizeForLogging());

            return new ApiIntegrationCredentialsModel
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                Name = name,
            };
        }
        catch (Exception ex) when (ex is not ConflictException)
        {
            logger.LogError(ex, "Failed to create API integration with client ID: {ClientId}", clientId.SanitizeForLogging());
            throw new InvalidOperationException($"Failed to create API integration: {ex.Message}", ex);
        }
    }

    public async Task<List<ApiIntegrationModel>> GetApiIntegrationsAsync()
    {
        try
        {
            logger.LogDebug("Fetching all clients from Keycloak realm: {Realm}", keycloakClient.Realm);

            var response = await keycloakClient.GetAsync($"admin/realms/{keycloakClient.Realm}/clients");
            response.EnsureSuccessStatusCode();

            var clients = await response.Content.ReadFromJsonAsync<List<KeycloakClient>>(keycloakClient.JsonOptions)
                ?? new List<KeycloakClient>();

            var filtered = new List<KeycloakClient>();
            foreach (var c in clients
                .Where(c => c.ClientId?.StartsWith(_options.ApiClientPrefix) == true)
                .Where(c => !string.IsNullOrEmpty(c.Id)))
            {
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

            logger.LogInformation("Found {Count} API integrations in tenant {TenantId}", apiIntegrations.Count, userContextProvider.TenantId);

            return apiIntegrations;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve API integrations from Keycloak");
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
            var clients = await GetClientsByClientIdAsync(clientId);
            var client = clients.FirstOrDefault();

            if (client == null)
            {
                logger.LogWarning("Attempted to delete non-existent API integration: {ClientId}", clientId.SanitizeForLogging());
                throw new NotFoundException($"API integration with client ID '{clientId}' not found.");
            }

            if (!await IsClientServiceAccountMemberOfCurrentOrganizationAsync(client.Id!))
            {
                logger.LogWarning("Attempted to delete API integration outside of tenant {TenantId}: {ClientId}", userContextProvider.TenantId, clientId.SanitizeForLogging());
                throw new InvalidOperationException("Forbidden: client does not belong to current tenant.");
            }

            logger.LogInformation("Deleting API integration: {ClientId} (internal ID: {InternalId})", clientId.SanitizeForLogging(), client.Id);
            
            var response = await keycloakClient.DeleteAsync($"admin/realms/{keycloakClient.Realm}/clients/{client.Id}");
            response.EnsureSuccessStatusCode();

            logger.LogInformation("Successfully deleted API integration: {ClientId}", clientId.SanitizeForLogging());
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
            logger.LogError(ex, "Failed to delete API integration: {ClientId}", clientId.SanitizeForLogging());
            throw new InvalidOperationException($"Failed to delete API integration: {ex.Message}", ex);
        }
    }

    public async Task<bool> ApiIntegrationExistsAsync(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            return false;

        try
        {
            var clients = await GetClientsByClientIdAsync(clientId);
            return clients.Any(c => c.ClientId == clientId && c.ClientId.StartsWith(_options.ApiClientPrefix));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check if API integration exists: {ClientId}", clientId.SanitizeForLogging());
            return false;
        }
    }

    private async Task AssignIntegrationRoleToServiceAccountAsync(string clientUuid)
    {
        try
        {
            var serviceAccountResponse = await keycloakClient.GetAsync(
                $"admin/realms/{keycloakClient.Realm}/clients/{clientUuid}/service-account-user");
            serviceAccountResponse.EnsureSuccessStatusCode();

            var serviceAccountUser = await serviceAccountResponse.Content.ReadFromJsonAsync<KeycloakServiceAccountUser>(keycloakClient.JsonOptions);
            var serviceAccountUserId = serviceAccountUser?.Id
                ?? throw new InvalidOperationException("Failed to retrieve service account user ID.");

            var rolesResponse = await keycloakClient.GetAsync(
                $"admin/realms/{keycloakClient.Realm}/roles/integration");
            rolesResponse.EnsureSuccessStatusCode();

            var integrationRole = await rolesResponse.Content.ReadFromJsonAsync<KeycloakRole>(keycloakClient.JsonOptions);
            if (integrationRole == null)
                throw new InvalidOperationException("Integration role not found in Keycloak.");

            var roleMapping = new List<KeycloakRole> { integrationRole };
            var assignRoleResponse = await keycloakClient.PostAsJsonAsync(
                $"admin/realms/{keycloakClient.Realm}/users/{serviceAccountUserId}/role-mappings/realm",
                roleMapping);
            assignRoleResponse.EnsureSuccessStatusCode();

            logger.LogInformation("Assigned integration role to service account for client: {ClientUuid}", clientUuid);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to assign integration role to service account for client: {ClientUuid}", clientUuid);
            throw new InvalidOperationException("Failed to assign integration role to service account.", ex);
        }
    }

    private async Task AddUserToOrganizationAsync(Guid organizationId, string userId)
    {
        var url = $"admin/realms/{keycloakClient.Realm}/organizations/{organizationId}/members";
        var json = JsonSerializer.Serialize(userId);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await keycloakClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            logger.LogError("Failed to add user {UserId} to organization {OrganizationId}: {StatusCode} - {Error}",
                userId, organizationId, response.StatusCode, errorContent);
            throw new InvalidOperationException($"Failed to add user to organization: {response.StatusCode}");
        }

        logger.LogInformation("Added user {UserId} to organization {OrganizationId}", userId, organizationId);
    }

    private async Task<bool> IsClientServiceAccountMemberOfCurrentOrganizationAsync(string clientUuid)
    {
        if (!userContextProvider.TenantId.HasValue)
            return false;

        try
        {
            var serviceAccountResponse = await keycloakClient.GetAsync(
                $"admin/realms/{keycloakClient.Realm}/clients/{clientUuid}/service-account-user");
            
            if (!serviceAccountResponse.IsSuccessStatusCode)
                return false;

            var serviceAccountUser = await serviceAccountResponse.Content.ReadFromJsonAsync<KeycloakServiceAccountUser>(keycloakClient.JsonOptions);
            var userId = serviceAccountUser?.Id;
            if (string.IsNullOrEmpty(userId))
                return false;

            var orgMembersResponse = await keycloakClient.GetAsync(
                $"admin/realms/{keycloakClient.Realm}/organizations/{userContextProvider.TenantId}/members");
            
            if (!orgMembersResponse.IsSuccessStatusCode)
                return false;

            var members = await orgMembersResponse.Content.ReadFromJsonAsync<List<JsonElement>>(keycloakClient.JsonOptions)
                ?? new List<JsonElement>();

            if (members.Any(m => m.TryGetProperty("id", out var idProp) && idProp.GetString() == userId))
                return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check if service account is member of current organization.");
            return false;
        }
        return false;
    }

    private async Task<List<KeycloakClient>> GetClientsByClientIdAsync(string clientId)
    {
        var response = await keycloakClient.GetAsync(
            $"admin/realms/{keycloakClient.Realm}/clients?clientId={Uri.EscapeDataString(clientId)}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<KeycloakClient>>(keycloakClient.JsonOptions)
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
}
