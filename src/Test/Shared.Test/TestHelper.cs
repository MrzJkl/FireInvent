using FireInvent.Contract;
using FireInvent.Database;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services.Keycloak;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Test.Shared;

internal static class TestHelper
{
    public static readonly Guid TestTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    internal static AppDbContext GetTestDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .UseLazyLoadingProxies()
            .Options;
        
        var testTenantProvider = new UserContextProvider
        {
            TenantId = TestTenantId,
            UserId = TestDataFactory.DefaultTestUserId
        };
        
        return new AppDbContext(options, testTenantProvider);
    }
}

/// <summary>
/// Mock implementation of IUserService for testing purposes.
/// </summary>
internal class MockUserService : IKeycloakUserService
{
    private readonly Dictionary<Guid, UserModel> _users = new();

    public void AddUser(Guid userId, string? firstName = null, string? lastName = null, string? email = null)
    {
        _users[userId] = new UserModel
        {
            Id = userId,
            FirstName = firstName,
            LastName = lastName,
            EMail = email ?? $"user-{userId}@test.com"
        };
    }

    public Task<List<UserModel>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.Values.ToList());
    }

    public Task<UserModel?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }
}
