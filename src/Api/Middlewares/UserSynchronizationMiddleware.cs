using FireInvent.Database;
using FireInvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace FireInvent.Api.Middlewares;

public class UserSynchronizationMiddleware(
    RequestDelegate next,
    ILogger<UserSynchronizationMiddleware> logger)
{
    private const string TokenCachePrefix = "user_sync_token";
    private static readonly TimeSpan TokenCacheExpiration = TimeSpan.FromMinutes(30);

    public async Task InvokeAsync(
        HttpContext context,
        AppDbContext dbContext,
        IMemoryCache cache)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await next(context);
            return;
        }

        try
        {
            var jtiClaim = context.User.FindFirst("jti");
            if (jtiClaim == null || string.IsNullOrWhiteSpace(jtiClaim.Value))
            {
                logger.LogDebug("No jti claim found in token, synchronizing user without cache.");
                await SynchronizeUser(context, dbContext);
                await next(context);
                return;
            }

            var cacheKey = $"{TokenCachePrefix}:{jtiClaim.Value}";
            
            if (cache.TryGetValue(cacheKey, out _))
            {
                await next(context);
                return;
            }

            await SynchronizeUser(context, dbContext);

            cache.Set(cacheKey, true, TokenCacheExpiration);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during user synchronization, continuing request.");
            // Continue with the request even if synchronization fails
        }

        await next(context);
    }

    private async Task SynchronizeUser(HttpContext context, AppDbContext dbContext)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            logger.LogWarning("Unable to extract user ID from token claims for synchronization.");
            return;
        }
        
        var userName = context.User.FindFirst("preferred_username")?.Value
            ?? context.User.FindFirst(ClaimTypes.Name)?.Value
            ?? context.User.FindFirst("name")?.Value
            ?? userId.ToString();

        var email = context.User.FindFirst(ClaimTypes.Email)?.Value;

        var firstName = context.User.FindFirst(ClaimTypes.GivenName)?.Value;

        var lastName = context.User.FindFirst(ClaimTypes.Surname)?.Value;

        var existingUser = await dbContext.Set<User>()
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync();

        if (existingUser == null)
        {
            var newUser = new User
            {
                Id = userId,
                UserName = userName,
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };

            await dbContext.Set<User>().AddAsync(newUser);

            try
            {
                await dbContext.SaveChangesAsync();
                
                logger.LogInformation("Created new user {UserId} ({UserName}) from token claims.", userId, userName);
            }
            catch (DbUpdateException ex)
            {
                // Handle possible concurrent creation of the same user
                var userNowExists = await dbContext.Set<User>()
                    .AnyAsync(u => u.Id == userId);

                if (userNowExists)
                {
                    logger.LogInformation(
                        ex,
                        "User {UserId} ({UserName}) was created concurrently during synchronization.",
                        userId,
                        userName);
                }
                else
                {
                    logger.LogError(
                        ex,
                        "Failed to create user {UserId} ({UserName}) during synchronization.",
                        userId,
                        userName);
                    throw;
                }
            }
        }
        else
        {
            bool hasChanges = false;

            if (!string.Equals(existingUser.UserName, userName, StringComparison.Ordinal))
            {
                existingUser.UserName = userName;
                hasChanges = true;
            }

            if (!string.Equals(existingUser.Email, email, StringComparison.Ordinal))
            {
                existingUser.Email = email;
                hasChanges = true;
            }

            if (!string.Equals(existingUser.FirstName, firstName, StringComparison.Ordinal))
            {
                existingUser.FirstName = firstName;
                hasChanges = true;
            }

            if (!string.Equals(existingUser.LastName, lastName, StringComparison.Ordinal))
            {
                existingUser.LastName = lastName;
                hasChanges = true;
            }

            if (hasChanges)
            {
                await dbContext.SaveChangesAsync();
                
                logger.LogInformation("Updated user {UserId} ({UserName}) from token claims.", userId, userName);
            }

            logger.LogDebug("User {UserId} ({UserName}) data is up to date.", userId, userName);
        }
    }
}
