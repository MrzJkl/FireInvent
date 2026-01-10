namespace FireInvent.Shared.Services.Keycloak;

/// <summary>
/// Service for managing Keycloak realms for tenant provisioning.
/// This service operates at the system/master level to create and configure tenant realms.
/// </summary>
public interface IKeycloakTenantService
{
    Task<Guid> CreateTenantOrganizationAsync(string name, string? description, CancellationToken cancellationToken = default);

    Task UpdateTenantOrganizationNameAsync(Guid organizationId, string newName, string? newDescription, CancellationToken cancellationToken = default);

    Task DeleteTenantOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
}
