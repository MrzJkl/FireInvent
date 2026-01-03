using FireInvent.Shared.Models;
namespace FireInvent.Shared.Services.Keycloak;

public interface IKeycloakUserService
{
    Task<List<UserModel>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<UserModel?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
}