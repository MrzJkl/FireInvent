using FireInvent.Contract;
using FireInvent.Shared.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace FireInvent.Shared.Services.Keycloak;

public class KeycloakUserService(
    KeycloakHttpClient keycloakClient,
    ILogger<KeycloakUserService> logger,
    UserContextProvider userContextProvider) : IKeycloakUserService
{
    public async Task<UserModel?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!userContextProvider.TenantId.HasValue)
        {
            logger.LogWarning("Tenant ID is not set in TenantProvider. Cannot retrieve user.");
            return null;
        }

        try
        {
            var response = await keycloakClient.GetAsync(
                $"admin/realms/{Uri.EscapeDataString(keycloakClient.Realm)}/users/{id}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("User with ID {UserId} not found in Keycloak.", id);
                return null;
            }

            var keycloakUser = await response.Content.ReadFromJsonAsync<KeycloakUser>(keycloakClient.JsonOptions, cancellationToken);
            if (keycloakUser == null)
            {
                logger.LogWarning("Failed to deserialize user with ID {UserId} from Keycloak.", id);
                return null;
            }

            if (!await IsUserMemberOfCurrentOrganizationAsync(id.ToString(), cancellationToken))
            {
                logger.LogWarning("User with ID {UserId} is not a member of current organization {TenantId}.", id, userContextProvider.TenantId);
                return null;
            }

            return MapKeycloakUserToUserModel(keycloakUser);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve user with ID {UserId} from Keycloak.", id);
            throw;
        }
    }

    public async Task<List<UserModel>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        if (!userContextProvider.TenantId.HasValue)
        {
            logger.LogWarning("Tenant ID is not set in TenantProvider. Cannot retrieve users.");
            return [];
        }

        try
        {
            var response = await keycloakClient.GetAsync(
                $"admin/realms/{Uri.EscapeDataString(keycloakClient.Realm)}/organizations/{userContextProvider.TenantId}/members", cancellationToken);
            
            response.EnsureSuccessStatusCode();

            var keycloakUsers = await response.Content.ReadFromJsonAsync<List<KeycloakUser>>(keycloakClient.JsonOptions, cancellationToken)
                ?? new List<KeycloakUser>();

            var userModels = keycloakUsers
                .Select(MapKeycloakUserToUserModel)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToList();

            logger.LogInformation("Retrieved {Count} users from Keycloak for tenant {TenantId}.", userModels.Count, userContextProvider.TenantId);

            return userModels;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve users from Keycloak for tenant {TenantId}.", userContextProvider.TenantId);
            throw;
        }
    }

    private async Task<bool> IsUserMemberOfCurrentOrganizationAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (!userContextProvider.TenantId.HasValue)
            return false;

        try
        {
            var response = await keycloakClient.GetAsync(
                $"admin/realms/{Uri.EscapeDataString(keycloakClient.Realm)}/organizations/{userContextProvider.TenantId}/members/{userId}", cancellationToken);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check if user {UserId} is member of organization {TenantId}.", userId, userContextProvider.TenantId);
            return false;
        }
    }

    private static UserModel MapKeycloakUserToUserModel(KeycloakUser keycloakUser)
    {
        var useBothNamesFromUsername = keycloakUser.FirstName is null && keycloakUser.LastName is null;
        
        return new UserModel
        {
            Id = Guid.Parse(keycloakUser.Id ?? throw new InvalidOperationException("User ID is missing.")),
            FirstName = useBothNamesFromUsername ? keycloakUser.Username : keycloakUser.FirstName,
            LastName = keycloakUser.LastName,
            EMail = keycloakUser.Email,
        };
    }
}
