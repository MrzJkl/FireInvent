using FireInvent.Shared.Models;
using System.Security.Claims;

namespace FireInvent.Shared.Services
{
    public interface IUserService
    {
        Task<List<UserModel>> GetAllUsersAsync();
        Task<UserModel?> GetUserByIdAsync(Guid id);
        Task<UserModel> SyncUserFromClaimsAsync(ClaimsPrincipal principal);
    }
}