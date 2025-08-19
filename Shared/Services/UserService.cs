using AutoMapper;
using FireInvent.Database;
using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FireInvent.Shared.Services;

public class UserService(AppDbContext context, IMapper mapper, ILogger<UserService> log) : IUserService
{
    public async Task<UserModel?> GetUserByIdAsync(Guid id)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        return mapper.Map<UserModel?>(user);
    }

    public async Task<List<UserModel>> GetAllUsersAsync()
    {
        var users = await context.Users
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .AsNoTracking()
            .ToListAsync();

        return mapper.Map<List<UserModel>>(users);
    }

    public async Task<UserModel> SyncUserFromClaimsAsync(ClaimsPrincipal principal)
    {
        var (id, firstname, lastname, email) = GetUserDetailsFromClaims(principal);
        var user = await context.Users.FindAsync(id);

        if (user is null)
        {
            user = new User()
            {
                Id = id,
                FirstName = firstname,
                LastName = lastname,
                EMail = email,
                CreatedAt = DateTimeOffset.UtcNow,
                LastLogin = DateTimeOffset.UtcNow,
            };

            context.Users.Add(user);

            log.LogInformation("Creating new user with ID {UserId} from claims.", id);
        }
        else
        {
            user.FirstName = firstname;
            user.LastName = lastname;
            user.EMail = email;
            user.LastLogin = DateTimeOffset.UtcNow;

            log.LogInformation("Updating existing user with ID {UserId} from claims.", id);
        }

        await Task.Delay(5000);

        await context.SaveChangesAsync();

        return mapper.Map<UserModel>(user);
    }

    private static (Guid id, string firstname, string lastname, string email) GetUserDetailsFromClaims(ClaimsPrincipal principal)
    {
        var firstName = principal.FindFirst(ClaimTypes.GivenName)?.Value
            ?? throw new InvalidOperationException("GivenName claim is missing or invalid.");

        var lastName = principal.FindFirst(ClaimTypes.Surname)?.Value
            ?? throw new InvalidOperationException("Surname claim is missing or invalid.");

        var email = principal.FindFirst(ClaimTypes.Email)?.Value
            ?? throw new InvalidOperationException("Email claim is missing or invalid.");

        var id = Guid.Parse(
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidOperationException("User ID claim is missing or invalid.")
        );

        return (id, firstName, lastName, email);
    }
}
