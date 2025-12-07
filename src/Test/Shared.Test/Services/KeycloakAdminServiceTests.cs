using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Options;
using FireInvent.Shared.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FireInvent.Test.Shared.Services;

public class KeycloakAdminServiceTests : IDisposable
{
    private readonly Mock<ILogger<KeycloakAdminService>> _loggerMock;
    private readonly HttpClient _httpClient;

    public KeycloakAdminServiceTests()
    {
        _loggerMock = new Mock<ILogger<KeycloakAdminService>>();
        _httpClient = new HttpClient();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Constructor_WithMissingUrl_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options = Options.Create(new KeycloakAdminOptions
        {
            Url = "",
            Realm = "test",
            AdminUsername = "admin",
            AdminPassword = "password"
        });

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => new KeycloakAdminService(_httpClient, options, _loggerMock.Object));
        Assert.Contains("URL", exception.Message);
    }

    [Fact]
    public void Constructor_WithMissingRealm_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options = Options.Create(new KeycloakAdminOptions
        {
            Url = "http://localhost:8080",
            Realm = "",
            AdminUsername = "admin",
            AdminPassword = "password"
        });

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => new KeycloakAdminService(_httpClient, options, _loggerMock.Object));
        Assert.Contains("realm", exception.Message);
    }

    [Fact]
    public void Constructor_WithMissingAdminUsername_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options = Options.Create(new KeycloakAdminOptions
        {
            Url = "http://localhost:8080",
            Realm = "test",
            AdminUsername = "",
            AdminPassword = "password"
        });

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => new KeycloakAdminService(_httpClient, options, _loggerMock.Object));
        Assert.Contains("username", exception.Message);
    }

    [Fact]
    public void Constructor_WithMissingAdminPassword_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options = Options.Create(new KeycloakAdminOptions
        {
            Url = "http://localhost:8080",
            Realm = "test",
            AdminUsername = "admin",
            AdminPassword = ""
        });

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => new KeycloakAdminService(_httpClient, options, _loggerMock.Object));
        Assert.Contains("password", exception.Message);
    }

    [Fact]
    public void Constructor_WithValidConfiguration_ShouldNotThrow()
    {
        // Arrange
        var options = Options.Create(new KeycloakAdminOptions
        {
            Url = "http://localhost:8080",
            Realm = "test",
            AdminUsername = "admin",
            AdminPassword = "password"
        });

        // Act & Assert
        var service = new KeycloakAdminService(_httpClient, options, _loggerMock.Object);
        Assert.NotNull(service);
    }
}
