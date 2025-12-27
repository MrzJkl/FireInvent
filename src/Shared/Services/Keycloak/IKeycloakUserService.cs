using FireInvent.Shared.Models;
namespace FireInvent.Shared.Services.Keycloak;

public interface IKeycloakUserService
{
    Task<List<UserModel>> GetAllUsersAsync();
    Task<UserModel?> GetUserByIdAsync(Guid id);
}