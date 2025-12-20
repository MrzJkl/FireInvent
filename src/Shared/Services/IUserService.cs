using FireInvent.Shared.Models;
namespace FireInvent.Shared.Services;

public interface IUserService
{
    Task<List<UserModel>> GetAllUsersAsync();
    Task<UserModel?> GetUserByIdAsync(Guid id);
}