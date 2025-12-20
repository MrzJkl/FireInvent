using FireInvent.Contract;
using FireInvent.Database;
using FireInvent.Database.Models;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FireInvent.Shared.Services;

public class UserService(AppDbContext context, ILogger<UserService> log, UserMapper mapper, TenantProvider tenantProvider) : IUserService
{
    public async Task<UserModel?> GetUserByIdAsync(Guid id)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        return user is null ? null : mapper.MapUserToUserModel(user);
    }

    public async Task<List<UserModel>> GetAllUsersAsync()
    {
        var users = await context.Users
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .AsNoTracking()
            .ToListAsync();

        return mapper.MapUsersToUserModels(users);
    }

    public async Task<UserModel?> SyncUserFromClaimsAsync(ClaimsPrincipal principal)
    {
        if (!tenantProvider.TenantId.HasValue)
        {
            log.LogDebug("Tenant ID is not set in TenantProvider. Skipping SyncUserFromClaimsAsync.");
            return null;
        }

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
                LastSync = DateTimeOffset.UtcNow,
            };

            await context.Users.AddAsync(user);

            log.LogInformation("Creating new user with ID {UserId} from claims.", id);
        }
        else
        {
            user.FirstName = firstname;
            user.LastName = lastname;
            user.EMail = email;
            user.LastSync = DateTimeOffset.UtcNow;

            log.LogInformation("Updating existing user with ID {UserId} from claims.", id);
        }

        await context.SaveChangesAsync();

        return mapper.MapUserToUserModel(user);
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
