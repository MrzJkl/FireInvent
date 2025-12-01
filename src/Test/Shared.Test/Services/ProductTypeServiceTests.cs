using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Services;

namespace FireInvent.Test.Shared.Services;

/// <summary>
/// Unit tests for ProductTypeService.
/// These tests focus on CRUD operations and duplicate name conflict detection.
/// </summary>
public class ProductTypeServiceTests
{
    private readonly ProductTypeMapper _mapper = new();

    [Fact]
    public async Task CreateProductTypeAsync_WithValidModel_ShouldCreateProductType()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ProductTypeService(context, _mapper);
        var model = TestDataFactory.CreateProductTypeModel("Helmet", "Head protection equipment");

        // Act
        var result = await service.CreateProductTypeAsync(model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.Name, result.Name);
        Assert.Equal(model.Description, result.Description);
    }

    [Fact]
    public async Task CreateProductTypeAsync_WithDuplicateName_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ProductTypeService(context, _mapper);
        var existingType = TestDataFactory.CreateProductType(name: "Helmet");
        context.ProductTypes.Add(existingType);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateProductTypeModel("Helmet");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.CreateProductTypeAsync(model));
    }

    [Fact]
    public async Task GetAllProductTypesAsync_ShouldReturnAllProductTypes_OrderedByName()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ProductTypeService(context, _mapper);
        context.ProductTypes.AddRange(
            TestDataFactory.CreateProductType(name: "Gloves"),
            TestDataFactory.CreateProductType(name: "Boots"),
            TestDataFactory.CreateProductType(name: "Helmet")
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllProductTypesAsync();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Boots", result[0].Name);
        Assert.Equal("Gloves", result[1].Name);
        Assert.Equal("Helmet", result[2].Name);
    }

    [Fact]
    public async Task GetAllProductTypesAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ProductTypeService(context, _mapper);

        // Act
        var result = await service.GetAllProductTypesAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetProductTypeByIdAsync_WithExistingId_ShouldReturnProductType()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ProductTypeService(context, _mapper);
        var productType = TestDataFactory.CreateProductType(name: "Jacket");
        context.ProductTypes.Add(productType);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetProductTypeByIdAsync(productType.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productType.Id, result.Id);
        Assert.Equal(productType.Name, result.Name);
    }

    [Fact]
    public async Task GetProductTypeByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ProductTypeService(context, _mapper);

        // Act
        var result = await service.GetProductTypeByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateProductTypeAsync_WithExistingId_ShouldUpdateProductType()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ProductTypeService(context, _mapper);
        var productType = TestDataFactory.CreateProductType(name: "Original Name");
        context.ProductTypes.Add(productType);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateProductTypeModel("Updated Name", "Updated Description");

        // Act
        var result = await service.UpdateProductTypeAsync(productType.Id, updateModel);

        // Assert
        Assert.True(result);
        var updated = await context.ProductTypes.FindAsync(productType.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal("Updated Description", updated.Description);
    }

    [Fact]
    public async Task UpdateProductTypeAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ProductTypeService(context, _mapper);
        var updateModel = TestDataFactory.CreateProductTypeModel("New Name");

        // Act
        var result = await service.UpdateProductTypeAsync(Guid.NewGuid(), updateModel);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateProductTypeAsync_WithDuplicateName_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ProductTypeService(context, _mapper);
        var existingType = TestDataFactory.CreateProductType(name: "Existing Name");
        var typeToUpdate = TestDataFactory.CreateProductType(name: "Original Name");
        context.ProductTypes.AddRange(existingType, typeToUpdate);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateProductTypeModel("Existing Name");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateProductTypeAsync(typeToUpdate.Id, updateModel));
    }

    [Fact]
    public async Task DeleteProductTypeAsync_WithExistingId_ShouldDeleteProductType()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ProductTypeService(context, _mapper);
        var productType = TestDataFactory.CreateProductType();
        context.ProductTypes.Add(productType);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteProductTypeAsync(productType.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await context.ProductTypes.FindAsync(productType.Id));
    }

    [Fact]
    public async Task DeleteProductTypeAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ProductTypeService(context, _mapper);

        // Act
        var result = await service.DeleteProductTypeAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }
}
