using FlameGuardLaundry.Shared.Exceptions;
using FlameGuardLaundry.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace FlameGuardLaundry.Shared.Services;

public class UserService(UserManager<IdentityUser> userManager)
{
    public async Task<List<UserModel>> GetAllUsersAsync()
    {
        var users = await userManager.Users
            .Select(u => new UserModel
            {
                Id = u.Id,
                Email = u.Email!,
                UserName = u.UserName!
            })
            .ToListAsync();

        return users;
    }

    public async Task<IdentityResult> CreateUserAsync(CreateUserModel model)
    {
        var user = new IdentityUser
        {
            UserName = model.UserName,
            Email = model.Email,
        };

        return await userManager.CreateAsync(user, model.Password);
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        await userManager.DeleteAsync(user);

        return true;
    }
}
