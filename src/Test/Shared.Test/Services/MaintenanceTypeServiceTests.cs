using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Services;

namespace FireInvent.Test.Shared.Services;

public class MaintenanceTypeServiceTests
{
    private readonly MaintenanceTypeMapper _mapper = new();

    [Fact]
    public async Task CreateMaintenanceTypeAsync_WithValidModel_ShouldCreateMaintenanceType()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new MaintenanceTypeService(context, _mapper);
        var model = TestDataFactory.CreateMaintenanceTypeModel("Annual Inspection", "Yearly equipment inspection");

        // Act
        var result = await service.CreateMaintenanceTypeAsync(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.Name, result.Name);
        Assert.Equal(model.Description, result.Description);
    }

    [Fact]
    public async Task CreateMaintenanceTypeAsync_WithDuplicateName_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new MaintenanceTypeService(context, _mapper);
        var existingType = TestDataFactory.CreateMaintenanceType(name: "Cleaning");
        context.MaintenanceTypes.Add(existingType);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateMaintenanceTypeModel("Cleaning");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.CreateMaintenanceTypeAsync(model));
    }

    [Fact]
    public async Task GetAllMaintenanceTypesAsync_ShouldReturnAllMaintenanceTypes_OrderedByName()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new MaintenanceTypeService(context, _mapper);
        context.MaintenanceTypes.AddRange(
            TestDataFactory.CreateMaintenanceType(name: "Repair"),
            TestDataFactory.CreateMaintenanceType(name: "Cleaning"),
            TestDataFactory.CreateMaintenanceType(name: "Inspection")
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllMaintenanceTypesAsync();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Cleaning", result[0].Name);
        Assert.Equal("Inspection", result[1].Name);
        Assert.Equal("Repair", result[2].Name);
    }

    [Fact]
    public async Task GetAllMaintenanceTypesAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new MaintenanceTypeService(context, _mapper);

        // Act
        var result = await service.GetAllMaintenanceTypesAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMaintenanceTypeByIdAsync_WithExistingId_ShouldReturnMaintenanceType()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new MaintenanceTypeService(context, _mapper);
        var maintenanceType = TestDataFactory.CreateMaintenanceType(name: "Calibration");
        context.MaintenanceTypes.Add(maintenanceType);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetMaintenanceTypeByIdAsync(maintenanceType.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(maintenanceType.Id, result.Id);
        Assert.Equal(maintenanceType.Name, result.Name);
    }

    [Fact]
    public async Task GetMaintenanceTypeByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new MaintenanceTypeService(context, _mapper);

        // Act
        var result = await service.GetMaintenanceTypeByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateMaintenanceTypeAsync_WithExistingId_ShouldUpdateMaintenanceType()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new MaintenanceTypeService(context, _mapper);
        var maintenanceType = TestDataFactory.CreateMaintenanceType(name: "Original Name");
        context.MaintenanceTypes.Add(maintenanceType);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateMaintenanceTypeModel("Updated Name", "Updated Description");

        // Act
        var result = await service.UpdateMaintenanceTypeAsync(maintenanceType.Id, updateModel);

        // Assert
        Assert.True(result);
        var updated = await context.MaintenanceTypes.FindAsync(maintenanceType.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal("Updated Description", updated.Description);
    }

    [Fact]
    public async Task UpdateMaintenanceTypeAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new MaintenanceTypeService(context, _mapper);
        var updateModel = TestDataFactory.CreateMaintenanceTypeModel("New Name");

        // Act
        var result = await service.UpdateMaintenanceTypeAsync(Guid.NewGuid(), updateModel);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateMaintenanceTypeAsync_WithDuplicateName_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new MaintenanceTypeService(context, _mapper);
        var existingType = TestDataFactory.CreateMaintenanceType(name: "Existing Name");
        var typeToUpdate = TestDataFactory.CreateMaintenanceType(name: "Original Name");
        context.MaintenanceTypes.AddRange(existingType, typeToUpdate);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateMaintenanceTypeModel("Existing Name");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateMaintenanceTypeAsync(typeToUpdate.Id, updateModel));
    }

    [Fact]
    public async Task DeleteMaintenanceTypeAsync_WithExistingId_ShouldDeleteMaintenanceType()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new MaintenanceTypeService(context, _mapper);
        var maintenanceType = TestDataFactory.CreateMaintenanceType();
        context.MaintenanceTypes.Add(maintenanceType);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteMaintenanceTypeAsync(maintenanceType.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await context.MaintenanceTypes.FindAsync(maintenanceType.Id));
    }

    [Fact]
    public async Task DeleteMaintenanceTypeAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new MaintenanceTypeService(context, _mapper);

        // Act
        var result = await service.DeleteMaintenanceTypeAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }
}
