using FireInvent.Shared.Models;
namespace FireInvent.Shared.Services;

public interface IKeycloakUserService
{
    Task<List<UserModel>> GetAllUsersAsync();
    Task<UserModel?> GetUserByIdAsync(Guid id);
}