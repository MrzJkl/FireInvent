using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services.Keycloak;

public interface IKeycloakApiIntegrationService
{
    Task<ApiIntegrationCredentialsModel> CreateApiIntegrationAsync(string name, string? description = null, CancellationToken cancellationToken = default);

    Task<List<ApiIntegrationModel>> GetApiIntegrationsAsync(CancellationToken cancellationToken = default);

    Task DeleteApiIntegrationAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ApiIntegrationExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
