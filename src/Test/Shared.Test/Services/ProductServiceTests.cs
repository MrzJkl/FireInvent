using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Test.Shared.Services;

/// <summary>
/// Unit tests for ProductService.
/// Note: Tests that verify returned mapped models are limited because InMemory database
/// doesn't populate navigation properties when re-querying entities, which the mappers require.
/// These tests focus on business logic (validation, conflict checking) and data persistence.
/// </summary>
public class ProductServiceTests
{
    private readonly ProductMapper _mapper = new();

    [Fact]
    public async Task CreateProductAsync_WithNonExistingProductType_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ProductService(context, _mapper);
        var model = TestDataFactory.CreateProductModel(Guid.NewGuid(), "Product", "Manufacturer");

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateProductAsync(model));
    }

    [Fact]
    public async Task CreateProductAsync_WithDuplicateNameAndManufacturer_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ProductService(context, _mapper);
        var productType = TestDataFactory.CreateProductType(name: "Helmet");
        var existingProduct = TestDataFactory.CreateProduct(productType.Id, name: "Product X", manufacturer: "BrandA");
        context.ProductTypes.Add(productType);
        context.Products.Add(existingProduct);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateProductModel(productType.Id, "Product X", "BrandA");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.CreateProductAsync(model));
    }

    [Fact]
    public async Task GetAllProductsAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ProductService(context, _mapper);

        // Act
        var result = await service.GetAllProductsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetProductByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ProductService(context, _mapper);

        // Act
        var result = await service.GetProductByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateProductAsync_WithExistingId_ShouldUpdateProduct()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ProductService(context, _mapper);
        var productType = TestDataFactory.CreateProductType(name: "Helmet");
        var product = TestDataFactory.CreateProduct(productType.Id, name: "Original Name", manufacturer: "BrandA");
        context.ProductTypes.Add(productType);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateProductModel(productType.Id, "Updated Name", "BrandB", "Updated Description");

        // Act
        var result = await service.UpdateProductAsync(product.Id, updateModel);

        // Assert
        Assert.True(result);
        var updated = await context.Products.FindAsync(product.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal("BrandB", updated.Manufacturer);
        Assert.Equal("Updated Description", updated.Description);
    }

    [Fact]
    public async Task UpdateProductAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ProductService(context, _mapper);
        var productType = TestDataFactory.CreateProductType(name: "Helmet");
        context.ProductTypes.Add(productType);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateProductModel(productType.Id, "New Name", "Brand");

        // Act
        var result = await service.UpdateProductAsync(Guid.NewGuid(), updateModel);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateProductAsync_WithNonExistingProductType_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ProductService(context, _mapper);
        var productType = TestDataFactory.CreateProductType(name: "Helmet");
        var product = TestDataFactory.CreateProduct(productType.Id, name: "Product");
        context.ProductTypes.Add(productType);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateProductModel(Guid.NewGuid(), "Updated", "Brand");

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateProductAsync(product.Id, updateModel));
    }

    [Fact]
    public async Task UpdateProductAsync_WithDuplicateNameAndManufacturer_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ProductService(context, _mapper);
        var productType = TestDataFactory.CreateProductType(name: "Helmet");
        var existingProduct = TestDataFactory.CreateProduct(productType.Id, name: "Existing", manufacturer: "BrandA");
        var productToUpdate = TestDataFactory.CreateProduct(productType.Id, name: "Original", manufacturer: "BrandB");
        context.ProductTypes.Add(productType);
        context.Products.AddRange(existingProduct, productToUpdate);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateProductModel(productType.Id, "Existing", "BrandA");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateProductAsync(productToUpdate.Id, updateModel));
    }

    [Fact]
    public async Task DeleteProductAsync_WithExistingId_ShouldDeleteProduct()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ProductService(context, _mapper);
        var productType = TestDataFactory.CreateProductType(name: "Helmet");
        var product = TestDataFactory.CreateProduct(productType.Id);
        context.ProductTypes.Add(productType);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteProductAsync(product.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await context.Products.FindAsync(product.Id));
    }

    [Fact]
    public async Task DeleteProductAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ProductService(context, _mapper);

        // Act
        var result = await service.DeleteProductAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }
}
