using FireInvent.Shared.Mapper;
using FireInvent.Shared.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace FireInvent.Test.Shared.Services;

/// <summary>
/// Unit tests for UserService.
/// These tests focus on user retrieval and claims-based user synchronization.
/// </summary>
public class UserServiceTests
{
    private readonly UserMapper _mapper = new();
    private readonly Mock<ILogger<UserService>> _mockLogger = new();

    private static ClaimsPrincipal CreateClaimsPrincipal(Guid id, string firstName, string lastName, string email)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            new Claim(ClaimTypes.GivenName, firstName),
            new Claim(ClaimTypes.Surname, lastName),
            new Claim(ClaimTypes.Email, email)
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    [Fact]
    public async Task GetUserByIdAsync_WithExistingId_ShouldReturnUser()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new UserService(context, _mockLogger.Object, _mapper);
        var user = TestDataFactory.CreateUser(email: "test@test.com", firstName: "Test", lastName: "User");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUserByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("Test", result.FirstName);
        Assert.Equal("User", result.LastName);
        Assert.Equal("test@test.com", result.EMail);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new UserService(context, _mockLogger.Object, _mapper);

        // Act
        var result = await service.GetUserByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllUsersAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new UserService(context, _mockLogger.Object, _mapper);

        // Act
        var result = await service.GetAllUsersAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnAllUsers_OrderedByLastNameThenFirstName()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new UserService(context, _mockLogger.Object, _mapper);
        context.Users.AddRange(
            TestDataFactory.CreateUser(email: "c@test.com", firstName: "Zara", lastName: "Brown"),
            TestDataFactory.CreateUser(email: "a@test.com", firstName: "Alice", lastName: "Brown"),
            TestDataFactory.CreateUser(email: "b@test.com", firstName: "Bob", lastName: "Anderson")
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllUsersAsync();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Bob", result[0].FirstName);
        Assert.Equal("Anderson", result[0].LastName);
        Assert.Equal("Alice", result[1].FirstName);
        Assert.Equal("Brown", result[1].LastName);
        Assert.Equal("Zara", result[2].FirstName);
        Assert.Equal("Brown", result[2].LastName);
    }

    [Fact]
    public async Task SyncUserFromClaimsAsync_WithNewUser_ShouldCreateUser()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new UserService(context, _mockLogger.Object, _mapper);
        var userId = Guid.NewGuid();
        var principal = CreateClaimsPrincipal(userId, "New", "User", "new@test.com");

        // Act
        var result = await service.SyncUserFromClaimsAsync(principal);

        // Assert
        Assert.Equal(userId, result.Id);
        Assert.Equal("New", result.FirstName);
        Assert.Equal("User", result.LastName);
        Assert.Equal("new@test.com", result.EMail);

        var savedUser = await context.Users.FindAsync(userId);
        Assert.NotNull(savedUser);
        Assert.Equal("new@test.com", savedUser.EMail);
    }

    [Fact]
    public async Task SyncUserFromClaimsAsync_WithExistingUser_ShouldUpdateUser()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new UserService(context, _mockLogger.Object, _mapper);
        var userId = Guid.NewGuid();
        var existingUser = TestDataFactory.CreateUser(id: userId, email: "old@test.com", firstName: "Old", lastName: "User");
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var principal = CreateClaimsPrincipal(userId, "Updated", "Name", "new@test.com");

        // Act
        var result = await service.SyncUserFromClaimsAsync(principal);

        // Assert
        Assert.Equal(userId, result.Id);
        Assert.Equal("Updated", result.FirstName);
        Assert.Equal("Name", result.LastName);
        Assert.Equal("new@test.com", result.EMail);

        var updatedUser = await context.Users.FindAsync(userId);
        Assert.NotNull(updatedUser);
        Assert.Equal("Updated", updatedUser.FirstName);
        Assert.Equal("Name", updatedUser.LastName);
        Assert.Equal("new@test.com", updatedUser.EMail);
    }

    [Fact]
    public async Task SyncUserFromClaimsAsync_WithMissingGivenNameClaim_ShouldThrow()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new UserService(context, _mockLogger.Object, _mapper);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Surname, "User"),
            new Claim(ClaimTypes.Email, "test@test.com")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SyncUserFromClaimsAsync(principal));
    }

    [Fact]
    public async Task SyncUserFromClaimsAsync_WithMissingSurnameClaim_ShouldThrow()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new UserService(context, _mockLogger.Object, _mapper);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.GivenName, "Test"),
            new Claim(ClaimTypes.Email, "test@test.com")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SyncUserFromClaimsAsync(principal));
    }

    [Fact]
    public async Task SyncUserFromClaimsAsync_WithMissingEmailClaim_ShouldThrow()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new UserService(context, _mockLogger.Object, _mapper);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.GivenName, "Test"),
            new Claim(ClaimTypes.Surname, "User")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SyncUserFromClaimsAsync(principal));
    }

    [Fact]
    public async Task SyncUserFromClaimsAsync_WithMissingNameIdentifierClaim_ShouldThrow()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new UserService(context, _mockLogger.Object, _mapper);
        var claims = new[]
        {
            new Claim(ClaimTypes.GivenName, "Test"),
            new Claim(ClaimTypes.Surname, "User"),
            new Claim(ClaimTypes.Email, "test@test.com")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SyncUserFromClaimsAsync(principal));
    }
}
