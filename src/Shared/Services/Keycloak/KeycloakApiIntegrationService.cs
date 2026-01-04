using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Contract.Extensions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;

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

    public async Task<ApiIntegrationCredentialsModel> CreateApiIntegrationAsync(string name, string? description = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        var clientId = $"{_options.ApiClientPrefix}-{userContextProvider.TenantId}-{SanitizeClientId(name)}";

        var existingClients = await GetClientsByClientIdAsync(clientId, cancellationToken);
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

            using var response = await keycloakClient.PostAsJsonAsync(
                $"admin/realms/{keycloakClient.Realm}/clients", 
                client);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to create client: {response.StatusCode} - {errorContent}");
            }

            var createdClients = await GetClientsByClientIdAsync(clientId, cancellationToken);
            var createdClient = createdClients.FirstOrDefault()
                ?? throw new InvalidOperationException("Failed to retrieve the created client.");

            await AssignIntegrationRoleToServiceAccountAsync(createdClient.Id!, cancellationToken);

            using var serviceAccountResponse = await keycloakClient.GetAsync(
                $"admin/realms/{keycloakClient.Realm}/clients/{createdClient.Id}/service-account-user");
            serviceAccountResponse.EnsureSuccessStatusCode();
            var serviceAccountUser = await serviceAccountResponse.Content.ReadFromJsonAsync<KeycloakServiceAccountUser>(keycloakClient.JsonOptions);
            var serviceAccountUserId = serviceAccountUser?.Id
                ?? throw new InvalidOperationException("Failed to retrieve service account user ID.");

            await AddUserToOrganizationAsync(userContextProvider.TenantId.Value, serviceAccountUserId, cancellationToken);

            using var secretResponse = await keycloakClient.GetAsync(
                $"admin/realms/{keycloakClient.Realm}/clients/{createdClient.Id}/client-secret");
            secretResponse.EnsureSuccessStatusCode();

            var credentials = await secretResponse.Content.ReadFromJsonAsync<KeycloakClientSecretResponse>(keycloakClient.JsonOptions);
            var clientSecret = credentials?.Value
                ?? throw new InvalidOperationException("Failed to retrieve the client secret.");

            logger.LogInformation("Successfully created API integration: {ClientId} with ID: {Id}", clientId.SanitizeForLogging(), createdClient.Id);

            return new ApiIntegrationCredentialsModel
            {
                Id = Guid.Parse(createdClient.Id),
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

    public async Task<List<ApiIntegrationModel>> GetApiIntegrationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Fetching all clients from Keycloak realm: {Realm}", keycloakClient.Realm);

            using var response = await keycloakClient.GetAsync($"admin/realms/{keycloakClient.Realm}/clients");
            response.EnsureSuccessStatusCode();

            var clients = await response.Content.ReadFromJsonAsync<List<KeycloakClient>>(keycloakClient.JsonOptions)
                ?? new List<KeycloakClient>();

            var filtered = new List<KeycloakClient>();
            foreach (var c in clients
                .Where(c => c.ClientId?.StartsWith(_options.ApiClientPrefix) == true)
                .Where(c => !string.IsNullOrEmpty(c.Id)))
            {
                if (await IsClientServiceAccountMemberOfCurrentOrganizationAsync(c.Id, cancellationToken))
                {
                    filtered.Add(c);
                }
            }

            var apiIntegrations = filtered
                .Select(c => new ApiIntegrationModel
                {
                    Id = Guid.Parse(c.Id!),
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

    public async Task DeleteApiIntegrationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var clientUuid = id.ToString();

        try
        {
            using var response = await keycloakClient.GetAsync(
                $"admin/realms/{keycloakClient.Realm}/clients/{clientUuid}");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Attempted to delete non-existent API integration with ID: {Id}", id);
                throw new NotFoundException($"API integration with ID '{id}' not found.");
            }

            var client = await response.Content.ReadFromJsonAsync<KeycloakClient>(keycloakClient.JsonOptions);

            if (client == null || !client.ClientId.StartsWith(_options.ApiClientPrefix))
            {
                logger.LogWarning("Attempted to delete client that is not an API integration: {Id}", id);
                throw new InvalidOperationException("Client is not an API integration.");
            }

            if (!await IsClientServiceAccountMemberOfCurrentOrganizationAsync(clientUuid, cancellationToken))
            {
                logger.LogWarning("Attempted to delete API integration outside of tenant {TenantId}: {Id}", userContextProvider.TenantId, id);
                throw new InvalidOperationException("Forbidden: client does not belong to current tenant.");
            }

            logger.LogInformation("Deleting API integration: {ClientId} (ID: {Id})", client.ClientId.SanitizeForLogging(), id);

            using var deleteResponse = await keycloakClient.DeleteAsync($"admin/realms/{keycloakClient.Realm}/clients/{clientUuid}");
            deleteResponse.EnsureSuccessStatusCode();

            logger.LogInformation("Successfully deleted API integration: {ClientId} (ID: {Id})", client.ClientId.SanitizeForLogging(), id);
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
            logger.LogError(ex, "Failed to delete API integration with ID: {Id}", id);
            throw new InvalidOperationException($"Failed to delete API integration: {ex.Message}", ex);
        }
    }

    public async Task<bool> ApiIntegrationExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var clientUuid = id.ToString();
            using var response = await keycloakClient.GetAsync(
                $"admin/realms/{keycloakClient.Realm}/clients/{clientUuid}");

            if (!response.IsSuccessStatusCode)
                return false;

            var client = await response.Content.ReadFromJsonAsync<KeycloakClient>(keycloakClient.JsonOptions);

            return client != null 
                && client.ClientId.StartsWith(_options.ApiClientPrefix)
                && await IsClientServiceAccountMemberOfCurrentOrganizationAsync(clientUuid, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check if API integration exists with ID: {Id}", id);
            return false;
        }
    }

    private async Task AssignIntegrationRoleToServiceAccountAsync(string clientUuid, CancellationToken cancellationToken = default)
    {
        try
        {
            using var serviceAccountResponse = await keycloakClient.GetAsync(
                $"admin/realms/{keycloakClient.Realm}/clients/{clientUuid}/service-account-user");
            serviceAccountResponse.EnsureSuccessStatusCode();

            var serviceAccountUser = await serviceAccountResponse.Content.ReadFromJsonAsync<KeycloakServiceAccountUser>(keycloakClient.JsonOptions);
            var serviceAccountUserId = serviceAccountUser?.Id
                ?? throw new InvalidOperationException("Failed to retrieve service account user ID.");

            using var rolesResponse = await keycloakClient.GetAsync(
                $"admin/realms/{keycloakClient.Realm}/roles/integration");
            rolesResponse.EnsureSuccessStatusCode();

            var integrationRole = await rolesResponse.Content.ReadFromJsonAsync<KeycloakRole>(keycloakClient.JsonOptions);
            if (integrationRole == null)
                throw new InvalidOperationException("Integration role not found in Keycloak.");

            var roleMapping = new List<KeycloakRole> { integrationRole };
            using var assignRoleResponse = await keycloakClient.PostAsJsonAsync(
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

    private async Task AddUserToOrganizationAsync(Guid organizationId, string userId, CancellationToken cancellationToken = default)
    {
        var url = $"admin/realms/{keycloakClient.Realm}/organizations/{organizationId}/members";
        var json = JsonSerializer.Serialize(userId);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await keycloakClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            logger.LogError("Failed to add user {UserId} to organization {OrganizationId}: {StatusCode} - {Error}",
                userId, organizationId, response.StatusCode, errorContent);
            throw new InvalidOperationException($"Failed to add user to organization: {response.StatusCode}");
        }

        logger.LogInformation("Added user {UserId} to organization {OrganizationId}", userId, organizationId);
    }

    private async Task<bool> IsClientServiceAccountMemberOfCurrentOrganizationAsync(string clientUuid, CancellationToken cancellationToken = default)
    {
        if (!userContextProvider.TenantId.HasValue)
            return false;

        try
        {
            using var serviceAccountResponse = await keycloakClient.GetAsync(
                $"admin/realms/{keycloakClient.Realm}/clients/{clientUuid}/service-account-user", cancellationToken);
            
            if (!serviceAccountResponse.IsSuccessStatusCode)
                return false;

            var serviceAccountUser = await serviceAccountResponse.Content.ReadFromJsonAsync<KeycloakServiceAccountUser>(keycloakClient.JsonOptions, cancellationToken);
            var userId = serviceAccountUser?.Id;
            if (string.IsNullOrEmpty(userId))
                return false;

            using var orgMembersResponse = await keycloakClient.GetAsync(
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

    private async Task<List<KeycloakClient>> GetClientsByClientIdAsync(string clientId, CancellationToken cancellationToken)
    {
        using var response = await keycloakClient.GetAsync(
            $"admin/realms/{keycloakClient.Realm}/clients?clientId={Uri.EscapeDataString(clientId)}", cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<KeycloakClient>>(keycloakClient.JsonOptions, cancellationToken)
            ?? [];
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
