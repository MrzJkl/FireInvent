using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Moq;

namespace FireInvent.Test.Shared.Services;

/// <summary>
/// Unit tests for MaintenanceService.
/// These tests focus on business logic (validation of related entities) and data persistence.
/// </summary>
public class MaintenanceServiceTests
{
    private readonly MaintenanceMapper _mapper = new();

    private async Task<(Guid ItemId, Guid MaintenanceTypeId, Guid UserId)> SetupBasicDataAsync(Database.AppDbContext context)
    {
        var productType = TestDataFactory.CreateProductType(name: "Helmet");
        var manufacturer = TestDataFactory.CreateManufacturer(name: "Test Manufacturer");
        var product = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id, name: "Safety Helmet");
        product.Type = productType;
        var variant = TestDataFactory.CreateVariant(product.Id, name: "Size L");
        variant.Product = product;
        var item = TestDataFactory.CreateItem(variant.Id);
        item.Variant = variant;
        var maintenanceType = TestDataFactory.CreateMaintenanceType(name: "Inspection");
        var user = TestDataFactory.CreateUser(email: "user@test.com", firstName: "Test", lastName: "User");

        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        context.Products.Add(product);
        context.Variants.Add(variant);
        context.Items.Add(item);
        context.MaintenanceTypes.Add(maintenanceType);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return (item.Id, maintenanceType.Id, user.Id);
    }

    [Fact]
    public async Task CreateMaintenanceAsync_WithNonExistingItem_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = new Mock<IUserService>();
        var service = new MaintenanceService(context, mockUserService.Object, _mapper);

        var maintenanceType = TestDataFactory.CreateMaintenanceType(name: "Inspection");
        context.MaintenanceTypes.Add(maintenanceType);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateMaintenanceModel(Guid.NewGuid(), maintenanceType.Id);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateMaintenanceAsync(model));
    }

    [Fact]
    public async Task CreateMaintenanceAsync_WithNonExistingMaintenanceType_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = new Mock<IUserService>();
        var service = new MaintenanceService(context, mockUserService.Object, _mapper);
        var (itemId, _, _) = await SetupBasicDataAsync(context);

        var model = TestDataFactory.CreateMaintenanceModel(itemId, Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateMaintenanceAsync(model));
    }

    [Fact]
    public async Task CreateMaintenanceAsync_WithNonExistingPerformedByUser_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = new Mock<IUserService>();
        mockUserService.Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>())).ReturnsAsync((UserModel?)null);
        var service = new MaintenanceService(context, mockUserService.Object, _mapper);
        var (itemId, maintenanceTypeId, _) = await SetupBasicDataAsync(context);

        var model = TestDataFactory.CreateMaintenanceModel(itemId, maintenanceTypeId, performedById: Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateMaintenanceAsync(model));
    }

    [Fact]
    public async Task GetAllMaintenancesAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = new Mock<IUserService>();
        var service = new MaintenanceService(context, mockUserService.Object, _mapper);

        // Act
        var result = await service.GetAllMaintenancesAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMaintenanceByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = new Mock<IUserService>();
        var service = new MaintenanceService(context, mockUserService.Object, _mapper);

        // Act
        var result = await service.GetMaintenanceByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateMaintenanceAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = new Mock<IUserService>();
        var service = new MaintenanceService(context, mockUserService.Object, _mapper);
        var (itemId, maintenanceTypeId, _) = await SetupBasicDataAsync(context);

        var updateModel = TestDataFactory.CreateMaintenanceModel(itemId, maintenanceTypeId);

        // Act
        var result = await service.UpdateMaintenanceAsync(Guid.NewGuid(), updateModel);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateMaintenanceAsync_WithNonExistingItem_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = new Mock<IUserService>();
        var service = new MaintenanceService(context, mockUserService.Object, _mapper);
        var (itemId, maintenanceTypeId, _) = await SetupBasicDataAsync(context);

        var maintenance = TestDataFactory.CreateMaintenance(itemId, maintenanceTypeId);
        context.Maintenances.Add(maintenance);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateMaintenanceModel(Guid.NewGuid(), maintenanceTypeId);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateMaintenanceAsync(maintenance.Id, updateModel));
    }

    [Fact]
    public async Task UpdateMaintenanceAsync_WithNonExistingMaintenanceType_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = new Mock<IUserService>();
        var service = new MaintenanceService(context, mockUserService.Object, _mapper);
        var (itemId, maintenanceTypeId, _) = await SetupBasicDataAsync(context);

        var maintenance = TestDataFactory.CreateMaintenance(itemId, maintenanceTypeId);
        context.Maintenances.Add(maintenance);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateMaintenanceModel(itemId, Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateMaintenanceAsync(maintenance.Id, updateModel));
    }

    [Fact]
    public async Task UpdateMaintenanceAsync_WithValidModel_ShouldUpdateMaintenance()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = new Mock<IUserService>();
        var service = new MaintenanceService(context, mockUserService.Object, _mapper);
        var (itemId, maintenanceTypeId, _) = await SetupBasicDataAsync(context);

        var maintenance = TestDataFactory.CreateMaintenance(itemId, maintenanceTypeId);
        context.Maintenances.Add(maintenance);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateMaintenanceModel(itemId, maintenanceTypeId, remarks: "Updated remarks");

        // Act
        var result = await service.UpdateMaintenanceAsync(maintenance.Id, updateModel);

        // Assert
        Assert.True(result);
        var updated = await context.Maintenances.FindAsync(maintenance.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated remarks", updated.Remarks);
    }

    [Fact]
    public async Task DeleteMaintenanceAsync_WithExistingId_ShouldDeleteMaintenance()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = new Mock<IUserService>();
        var service = new MaintenanceService(context, mockUserService.Object, _mapper);
        var (itemId, maintenanceTypeId, _) = await SetupBasicDataAsync(context);

        var maintenance = TestDataFactory.CreateMaintenance(itemId, maintenanceTypeId);
        context.Maintenances.Add(maintenance);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteMaintenanceAsync(maintenance.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await context.Maintenances.FindAsync(maintenance.Id));
    }

    [Fact]
    public async Task DeleteMaintenanceAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = new Mock<IUserService>();
        var service = new MaintenanceService(context, mockUserService.Object, _mapper);

        // Act
        var result = await service.DeleteMaintenanceAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetMaintenancesForItemAsync_WithNonExistingItem_ShouldThrowNotFoundException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = new Mock<IUserService>();
        var service = new MaintenanceService(context, mockUserService.Object, _mapper);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetMaintenancesForItemAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetMaintenancesForItemAsync_WithNoMaintenancesForItem_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var mockUserService = new Mock<IUserService>();
        var service = new MaintenanceService(context, mockUserService.Object, _mapper);
        var (itemId, _, _) = await SetupBasicDataAsync(context);

        // Act
        var result = await service.GetMaintenancesForItemAsync(itemId);

        // Assert
        Assert.Empty(result);
    }
}
