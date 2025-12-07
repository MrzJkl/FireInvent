using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

/// <summary>
/// Service for managing API integrations via Keycloak Admin API.
/// </summary>
public interface IKeycloakAdminService
{
    /// <summary>
    /// Creates a new confidential client in Keycloak for API integration.
    /// </summary>
    /// <param name="name">The user-defined name for the integration.</param>
    /// <param name="description">Optional description of the integration.</param>
    /// <returns>The credentials for the newly created integration.</returns>
    Task<ApiIntegrationCredentialsModel> CreateApiIntegrationAsync(string name, string? description = null);

    /// <summary>
    /// Lists all API integrations (confidential clients with the configured prefix).
    /// </summary>
    /// <returns>A list of API integrations.</returns>
    Task<List<ApiIntegrationModel>> GetApiIntegrationsAsync();

    /// <summary>
    /// Deletes an API integration by its client ID.
    /// </summary>
    /// <param name="clientId">The client ID of the integration to delete.</param>
    Task DeleteApiIntegrationAsync(string clientId);

    /// <summary>
    /// Checks if an API integration exists by its client ID.
    /// </summary>
    /// <param name="clientId">The client ID to check.</param>
    /// <returns>True if the integration exists, false otherwise.</returns>
    Task<bool> ApiIntegrationExistsAsync(string clientId);
}
