using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services.Keycloak;

public interface IKeycloakApiIntegrationService
{
    Task<ApiIntegrationCredentialsModel> CreateApiIntegrationAsync(string name, string? description = null);

    Task<List<ApiIntegrationModel>> GetApiIntegrationsAsync();

    Task DeleteApiIntegrationAsync(Guid id);

    Task<bool> ApiIntegrationExistsAsync(Guid id);
}
