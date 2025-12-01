using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Services;

namespace FireInvent.Test.Shared.Services;

/// <summary>
/// Unit tests for StorageLocationService.
/// These tests focus on CRUD operations and duplicate name conflict detection.
/// </summary>
public class StorageLocationServiceTests
{
    private readonly StorageLocationMapper _mapper = new();

    [Fact]
    public async Task CreateStorageLocationAsync_WithValidModel_ShouldCreateStorageLocation()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new StorageLocationService(context, _mapper);
        var model = TestDataFactory.CreateStorageLocationModel("Warehouse A", "Main storage");

        // Act
        var result = await service.CreateStorageLocationAsync(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.Name, result.Name);
        Assert.Equal(model.Remarks, result.Remarks);
    }

    [Fact]
    public async Task CreateStorageLocationAsync_WithDuplicateName_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new StorageLocationService(context, _mapper);
        var existingLocation = TestDataFactory.CreateStorageLocation(name: "Warehouse A");
        context.StorageLocations.Add(existingLocation);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateStorageLocationModel("Warehouse A");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.CreateStorageLocationAsync(model));
    }

    [Fact]
    public async Task GetAllStorageLocationsAsync_ShouldReturnAllStorageLocations_OrderedByName()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new StorageLocationService(context, _mapper);
        context.StorageLocations.AddRange(
            TestDataFactory.CreateStorageLocation(name: "Warehouse C"),
            TestDataFactory.CreateStorageLocation(name: "Warehouse A"),
            TestDataFactory.CreateStorageLocation(name: "Warehouse B")
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllStorageLocationsAsync();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Warehouse A", result[0].Name);
        Assert.Equal("Warehouse B", result[1].Name);
        Assert.Equal("Warehouse C", result[2].Name);
    }

    [Fact]
    public async Task GetAllStorageLocationsAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new StorageLocationService(context, _mapper);

        // Act
        var result = await service.GetAllStorageLocationsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetStorageLocationByIdAsync_WithExistingId_ShouldReturnStorageLocation()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new StorageLocationService(context, _mapper);
        var location = TestDataFactory.CreateStorageLocation(name: "Vehicle 1");
        context.StorageLocations.Add(location);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetStorageLocationByIdAsync(location.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(location.Id, result.Id);
        Assert.Equal(location.Name, result.Name);
    }

    [Fact]
    public async Task GetStorageLocationByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new StorageLocationService(context, _mapper);

        // Act
        var result = await service.GetStorageLocationByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStorageLocationAsync_WithExistingId_ShouldUpdateStorageLocation()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new StorageLocationService(context, _mapper);
        var location = TestDataFactory.CreateStorageLocation(name: "Original Name");
        context.StorageLocations.Add(location);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateStorageLocationModel("Updated Name", "Updated Remarks");

        // Act
        var result = await service.UpdateStorageLocationAsync(location.Id, updateModel);

        // Assert
        Assert.True(result);
        var updated = await context.StorageLocations.FindAsync(location.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal("Updated Remarks", updated.Remarks);
    }

    [Fact]
    public async Task UpdateStorageLocationAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new StorageLocationService(context, _mapper);
        var updateModel = TestDataFactory.CreateStorageLocationModel("New Name");

        // Act
        var result = await service.UpdateStorageLocationAsync(Guid.NewGuid(), updateModel);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateStorageLocationAsync_WithDuplicateName_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new StorageLocationService(context, _mapper);
        var existingLocation = TestDataFactory.CreateStorageLocation(name: "Existing Name");
        var locationToUpdate = TestDataFactory.CreateStorageLocation(name: "Original Name");
        context.StorageLocations.AddRange(existingLocation, locationToUpdate);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateStorageLocationModel("Existing Name");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateStorageLocationAsync(locationToUpdate.Id, updateModel));
    }

    [Fact]
    public async Task DeleteStorageLocationAsync_WithExistingId_ShouldDeleteStorageLocation()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new StorageLocationService(context, _mapper);
        var location = TestDataFactory.CreateStorageLocation();
        context.StorageLocations.Add(location);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteStorageLocationAsync(location.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await context.StorageLocations.FindAsync(location.Id));
    }

    [Fact]
    public async Task DeleteStorageLocationAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new StorageLocationService(context, _mapper);

        // Act
        var result = await service.DeleteStorageLocationAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }
}
