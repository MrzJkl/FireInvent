using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;

namespace FireInvent.Test.Shared.Services;

/// <summary>
/// Unit tests for ManufacturerService.
/// </summary>
public class ManufacturerServiceTests
{
    private readonly ManufacturerMapper _mapper = new();

    [Fact]
    public async Task CreateManufacturerAsync_WithValidData_ShouldCreateManufacturer()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ManufacturerService(context, _mapper);
        var model = TestDataFactory.CreateManufacturerModel("Test Manufacturer", "Test Description");

        // Act
        var result = await service.CreateManufacturerAsync(model);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Manufacturer", result.Name);
        Assert.Equal("Test Description", result.Description);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task CreateManufacturerAsync_WithDuplicateName_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ManufacturerService(context, _mapper);
        var existingManufacturer = TestDataFactory.CreateManufacturer(name: "BrandA");
        context.Manufacturers.Add(existingManufacturer);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateManufacturerModel("BrandA");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.CreateManufacturerAsync(model));
    }

    [Fact]
    public async Task GetAllManufacturersAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ManufacturerService(context, _mapper);
        var query = new PagedQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetAllManufacturersAsync(query, CancellationToken.None);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
    }

    [Fact]
    public async Task GetAllManufacturersAsync_WithMultipleManufacturers_ShouldReturnOrderedList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ManufacturerService(context, _mapper);
        var manufacturerB = TestDataFactory.CreateManufacturer(name: "BrandB");
        var manufacturerA = TestDataFactory.CreateManufacturer(name: "BrandA");
        var manufacturerC = TestDataFactory.CreateManufacturer(name: "BrandC");
        context.Manufacturers.AddRange(manufacturerB, manufacturerA, manufacturerC);
        await context.SaveChangesAsync();
        var query = new PagedQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetAllManufacturersAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Items.Count);
        Assert.Equal("BrandA", result.Items[0].Name);
        Assert.Equal("BrandB", result.Items[1].Name);
        Assert.Equal("BrandC", result.Items[2].Name);
        Assert.Equal(3, result.TotalItems);
    }

    [Fact]
    public async Task GetManufacturerByIdAsync_WithExistingId_ShouldReturnManufacturer()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ManufacturerService(context, _mapper);
        var manufacturer = TestDataFactory.CreateManufacturer(name: "BrandA", description: "Description A");
        context.Manufacturers.Add(manufacturer);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetManufacturerByIdAsync(manufacturer.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(manufacturer.Id, result.Id);
        Assert.Equal("BrandA", result.Name);
        Assert.Equal("Description A", result.Description);
    }

    [Fact]
    public async Task GetManufacturerByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ManufacturerService(context, _mapper);

        // Act
        var result = await service.GetManufacturerByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateManufacturerAsync_WithExistingId_ShouldUpdateManufacturer()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ManufacturerService(context, _mapper);
        var manufacturer = TestDataFactory.CreateManufacturer(name: "Original Name", description: "Original Description");
        context.Manufacturers.Add(manufacturer);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateManufacturerModel("Updated Name", "Updated Description");

        // Act
        var result = await service.UpdateManufacturerAsync(manufacturer.Id, updateModel);

        // Assert
        Assert.True(result);
        var updated = await context.Manufacturers.FindAsync(manufacturer.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal("Updated Description", updated.Description);
    }

    [Fact]
    public async Task UpdateManufacturerAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ManufacturerService(context, _mapper);
        var updateModel = TestDataFactory.CreateManufacturerModel("New Name");

        // Act
        var result = await service.UpdateManufacturerAsync(Guid.NewGuid(), updateModel);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateManufacturerAsync_WithDuplicateName_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ManufacturerService(context, _mapper);
        var existingManufacturer = TestDataFactory.CreateManufacturer(name: "BrandA");
        var manufacturerToUpdate = TestDataFactory.CreateManufacturer(name: "BrandB");
        context.Manufacturers.AddRange(existingManufacturer, manufacturerToUpdate);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateManufacturerModel("BrandA");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateManufacturerAsync(manufacturerToUpdate.Id, updateModel));
    }

    [Fact]
    public async Task DeleteManufacturerAsync_WithExistingId_ShouldDeleteManufacturer()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ManufacturerService(context, _mapper);
        var manufacturer = TestDataFactory.CreateManufacturer(name: "BrandA");
        context.Manufacturers.Add(manufacturer);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteManufacturerAsync(manufacturer.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await context.Manufacturers.FindAsync(manufacturer.Id));
    }

    [Fact]
    public async Task DeleteManufacturerAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ManufacturerService(context, _mapper);

        // Act
        var result = await service.DeleteManufacturerAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateManufacturerAsync_WithAllOptionalFields_ShouldCreateManufacturer()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ManufacturerService(context, _mapper);
        var model = new CreateOrUpdateManufacturerModel
        {
            Name = "Complete Manufacturer",
            Description = "Full Description",
            Street = "Main Street",
            HouseNumber = "123",
            City = "New York",
            PostalCode = "10001",
            Country = "USA",
            Website = "https://example.com",
            PhoneNumber = "+1-555-1234",
            Email = "contact@example.com"
        };

        // Act
        var result = await service.CreateManufacturerAsync(model);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Complete Manufacturer", result.Name);
        Assert.Equal("Main Street", result.Street);
        Assert.Equal("123", result.HouseNumber);
        Assert.Equal("New York", result.City);
        Assert.Equal("10001", result.PostalCode);
        Assert.Equal("USA", result.Country);
        Assert.Equal("https://example.com", result.Website);
        Assert.Equal("+1-555-1234", result.PhoneNumber);
        Assert.Equal("contact@example.com", result.Email);
    }
}
