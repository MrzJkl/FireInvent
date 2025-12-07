using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Options;
using Keycloak.Net;
using Keycloak.Net.Models.Clients;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FireInvent.Shared.Services;

/// <summary>
/// Service for managing API integrations via Keycloak Admin API.
/// </summary>
public class KeycloakAdminService : IKeycloakAdminService
{
    private readonly KeycloakClient _keycloakClient;
    private readonly KeycloakAdminOptions _options;
    private readonly ILogger<KeycloakAdminService> _logger;

    public KeycloakAdminService(
        IOptions<KeycloakAdminOptions> options,
        ILogger<KeycloakAdminService> logger)
    {
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.Url))
            throw new InvalidOperationException("Keycloak URL is not configured.");

        if (string.IsNullOrWhiteSpace(_options.Realm))
            throw new InvalidOperationException("Keycloak realm is not configured.");

        if (string.IsNullOrWhiteSpace(_options.AdminUsername))
            throw new InvalidOperationException("Keycloak admin username is not configured.");

        if (string.IsNullOrWhiteSpace(_options.AdminPassword))
            throw new InvalidOperationException("Keycloak admin password is not configured.");

        _keycloakClient = new KeycloakClient(
            _options.Url,
            _options.AdminUsername,
            _options.AdminPassword);
    }

    public async Task<ApiIntegrationCredentials> CreateApiIntegrationAsync(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        var clientId = $"{_options.ApiClientPrefix}{SanitizeClientId(name)}";

        // Check if a client with this ID already exists
        var existingClients = await _keycloakClient.GetClientsAsync(_options.Realm, clientId: clientId);
        if (existingClients.Any())
        {
            _logger.LogWarning("Attempted to create API integration with duplicate client ID: {ClientId}", clientId);
            throw new ConflictException($"An API integration with the name '{name}' already exists.");
        }

        var client = new Client
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
            Attributes = new Dictionary<string, object>
            {
                // Store the description in attributes since Description property doesn't exist
                ["description"] = description ?? string.Empty
            }
        };

        try
        {
            _logger.LogInformation("Creating API integration with client ID: {ClientId}", clientId);
            await _keycloakClient.CreateClientAsync(_options.Realm, client);

            // Retrieve the created client to get its internal ID and secret
            var createdClients = await _keycloakClient.GetClientsAsync(_options.Realm, clientId: clientId);
            var createdClient = createdClients.FirstOrDefault()
                ?? throw new InvalidOperationException("Failed to retrieve the created client.");

            // Get the client secret
            var credentials = await _keycloakClient.GetClientSecretAsync(_options.Realm, createdClient.Id!);
            var clientSecret = credentials?.Value
                ?? throw new InvalidOperationException("Failed to retrieve the client secret.");

            _logger.LogInformation("Successfully created API integration: {ClientId}", clientId);

            return new ApiIntegrationCredentials
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                Name = name,
                CreatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex) when (ex is not ConflictException)
        {
            _logger.LogError(ex, "Failed to create API integration with client ID: {ClientId}", clientId);
            throw new InvalidOperationException($"Failed to create API integration: {ex.Message}", ex);
        }
    }

    public async Task<List<ApiIntegrationListItem>> GetApiIntegrationsAsync()
    {
        try
        {
            _logger.LogDebug("Fetching all clients from Keycloak realm: {Realm}", _options.Realm);

            var clients = await _keycloakClient.GetClientsAsync(_options.Realm);

            var apiIntegrations = clients
                .Where(c => c.ClientId?.StartsWith(_options.ApiClientPrefix) == true)
                .Select(c => new ApiIntegrationListItem
                {
                    ClientId = c.ClientId!,
                    Name = c.Name ?? ExtractNameFromClientId(c.ClientId!),
                    Description = c.Attributes?.ContainsKey("description") == true 
                        ? c.Attributes["description"]?.ToString() 
                        : null,
                    Enabled = c.Enabled ?? false,
                    CreatedAt = null // Keycloak doesn't provide creation timestamp in client object
                })
                .OrderBy(i => i.Name)
                .ToList();

            _logger.LogInformation("Found {Count} API integrations", apiIntegrations.Count);

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
            // Find the client by client ID to get its internal ID
            var clients = await _keycloakClient.GetClientsAsync(_options.Realm, clientId: clientId);
            var client = clients.FirstOrDefault();

            if (client == null)
            {
                _logger.LogWarning("Attempted to delete non-existent API integration: {ClientId}", clientId);
                throw new NotFoundException($"API integration with client ID '{clientId}' not found.");
            }

            _logger.LogInformation("Deleting API integration: {ClientId} (internal ID: {InternalId})", clientId, client.Id);
            await _keycloakClient.DeleteClientAsync(_options.Realm, client.Id!);
            _logger.LogInformation("Successfully deleted API integration: {ClientId}", clientId);
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
            _logger.LogError(ex, "Failed to delete API integration: {ClientId}", clientId);
            throw new InvalidOperationException($"Failed to delete API integration: {ex.Message}", ex);
        }
    }

    public async Task<bool> ApiIntegrationExistsAsync(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            return false;

        try
        {
            var clients = await _keycloakClient.GetClientsAsync(_options.Realm, clientId: clientId);
            return clients.Any(c => c.ClientId == clientId && c.ClientId.StartsWith(_options.ApiClientPrefix));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if API integration exists: {ClientId}", clientId);
            return false;
        }
    }

    /// <summary>
    /// Sanitizes the user-provided name to create a valid client ID.
    /// </summary>
    private static string SanitizeClientId(string name)
    {
        // Replace spaces and special characters with hyphens, convert to lowercase
        var sanitized = new string(name
            .ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray());

        // Remove consecutive hyphens and trim
        while (sanitized.Contains("--"))
            sanitized = sanitized.Replace("--", "-");

        return sanitized.Trim('-');
    }

    /// <summary>
    /// Extracts a readable name from a client ID by removing the prefix and replacing hyphens.
    /// </summary>
    private string ExtractNameFromClientId(string clientId)
    {
        if (!clientId.StartsWith(_options.ApiClientPrefix))
            return clientId;

        return clientId[_options.ApiClientPrefix.Length..]
            .Replace('-', ' ')
            .Trim();
    }
}
