namespace FireInvent.Shared.Services;

/// <summary>
/// Service for managing Keycloak realms for tenant provisioning.
/// This service operates at the system/master level to create and configure tenant realms.
/// </summary>
public interface IKeycloakTenantService
{
    /// <summary>
    /// Creates a new Keycloak realm for a tenant.
    /// </summary>
    /// <param name="realmName">The name of the realm to create.</param>
    /// <param name="displayName">The display name for the realm.</param>
    /// <returns>True if the realm was created successfully, false otherwise.</returns>
    Task<bool> CreateTenantRealmAsync(string realmName, string displayName);

    /// <summary>
    /// Checks if a realm exists in Keycloak.
    /// </summary>
    /// <param name="realmName">The name of the realm to check.</param>
    /// <returns>True if the realm exists, false otherwise.</returns>
    Task<bool> RealmExistsAsync(string realmName);

    /// <summary>
    /// Deletes a Keycloak realm.
    /// </summary>
    /// <param name="realmName">The name of the realm to delete.</param>
    /// <returns>True if the realm was deleted successfully, false otherwise.</returns>
    Task<bool> DeleteTenantRealmAsync(string realmName);

    /// <summary>
    /// Configures a newly created realm with default settings for tenant use.
    /// </summary>
    /// <param name="realmName">The name of the realm to configure.</param>
    Task ConfigureTenantRealmAsync(string realmName);
}
