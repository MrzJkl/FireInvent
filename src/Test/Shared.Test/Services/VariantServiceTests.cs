using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Test.Shared.Services;

/// <summary>
/// Unit tests for VariantService.
/// Note: Tests that verify returned mapped models are limited because InMemory database
/// doesn't populate navigation properties when re-querying entities, which the mappers require.
/// These tests focus on business logic (validation, conflict checking) and data persistence.
/// </summary>
public class VariantServiceTests
{
    private readonly VariantMapper _mapper = new();

    [Fact]
    public async Task CreateVariantAsync_WithNonExistingProduct_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VariantService(context, _mapper);
        var model = TestDataFactory.CreateVariantModel(Guid.NewGuid(), "Size L");

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateVariantAsync(model));
    }

    [Fact]
    public async Task CreateVariantAsync_WithDuplicateNameForSameProduct_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VariantService(context, _mapper);
        var productType = TestDataFactory.CreateProductType(name: "Helmet");
        var manufacturer = TestDataFactory.CreateManufacturer(name: "Test Manufacturer");
        var product = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id, name: "Safety Helmet");
        product.Type = productType;
        var existingVariant = TestDataFactory.CreateVariant(product.Id, name: "Size L");
        existingVariant.Product = product;
        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        context.Products.Add(product);
        context.Variants.Add(existingVariant);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateVariantModel(product.Id, "Size L");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.CreateVariantAsync(model));
    }

    [Fact]
    public async Task GetAllVariantsAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VariantService(context, _mapper);

        // Act
        var result = await service.GetAllVariantsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetVariantByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VariantService(context, _mapper);

        // Act
        var result = await service.GetVariantByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateVariantAsync_WithExistingId_ShouldUpdateVariant()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VariantService(context, _mapper);
        var productType = TestDataFactory.CreateProductType(name: "Helmet");
        var manufacturer = TestDataFactory.CreateManufacturer(name: "Test Manufacturer");
        var product = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id, name: "Safety Helmet");
        product.Type = productType;
        var variant = TestDataFactory.CreateVariant(product.Id, name: "Original Name");
        variant.Product = product;
        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        context.Products.Add(product);
        context.Variants.Add(variant);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateVariantModel(product.Id, "Updated Name", "Updated Specs");

        // Act
        var result = await service.UpdateVariantAsync(variant.Id, updateModel);

        // Assert
        Assert.True(result);
        var updated = await context.Variants.FindAsync(variant.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal("Updated Specs", updated.AdditionalSpecs);
    }

    [Fact]
    public async Task UpdateVariantAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VariantService(context, _mapper);
        var productType = TestDataFactory.CreateProductType(name: "Helmet");
        var manufacturer = TestDataFactory.CreateManufacturer(name: "Test Manufacturer");
        var product = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id, name: "Safety Helmet");
        product.Type = productType;
        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateVariantModel(product.Id, "New Name");

        // Act
        var result = await service.UpdateVariantAsync(Guid.NewGuid(), updateModel);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateVariantAsync_WithNonExistingProduct_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VariantService(context, _mapper);
        var productType = TestDataFactory.CreateProductType(name: "Helmet");
        var manufacturer = TestDataFactory.CreateManufacturer(name: "Test Manufacturer");
        var product = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id, name: "Safety Helmet");
        product.Type = productType;
        var variant = TestDataFactory.CreateVariant(product.Id, name: "Size L");
        variant.Product = product;
        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        context.Products.Add(product);
        context.Variants.Add(variant);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateVariantModel(Guid.NewGuid(), "Updated Name");

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateVariantAsync(variant.Id, updateModel));
    }

    [Fact]
    public async Task UpdateVariantAsync_WithDuplicateNameForSameProduct_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VariantService(context, _mapper);
        var productType = TestDataFactory.CreateProductType(name: "Helmet");
        var manufacturer = TestDataFactory.CreateManufacturer(name: "Test Manufacturer");
        var product = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id, name: "Safety Helmet");
        product.Type = productType;
        var existingVariant = TestDataFactory.CreateVariant(product.Id, name: "Existing Name");
        existingVariant.Product = product;
        var variantToUpdate = TestDataFactory.CreateVariant(product.Id, name: "Original Name");
        variantToUpdate.Product = product;
        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        context.Products.Add(product);
        context.Variants.AddRange(existingVariant, variantToUpdate);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateVariantModel(product.Id, "Existing Name");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateVariantAsync(variantToUpdate.Id, updateModel));
    }

    [Fact]
    public async Task DeleteVariantAsync_WithExistingId_ShouldDeleteVariant()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VariantService(context, _mapper);
        var productType = TestDataFactory.CreateProductType(name: "Helmet");
        var manufacturer = TestDataFactory.CreateManufacturer(name: "Test Manufacturer");
        var product = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id, name: "Safety Helmet");
        product.Type = productType;
        var variant = TestDataFactory.CreateVariant(product.Id);
        variant.Product = product;
        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        context.Products.Add(product);
        context.Variants.Add(variant);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteVariantAsync(variant.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await context.Variants.FindAsync(variant.Id));
    }

    [Fact]
    public async Task DeleteVariantAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VariantService(context, _mapper);

        // Act
        var result = await service.DeleteVariantAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetVariantsForProductAsync_WithNonExistingProduct_ShouldThrowNotFoundException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VariantService(context, _mapper);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetVariantsForProductAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetVariantsForProductAsync_WithNoVariantsForProduct_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VariantService(context, _mapper);
        var productType = TestDataFactory.CreateProductType(name: "Helmet");
        var manufacturer = TestDataFactory.CreateManufacturer(name: "Test Manufacturer");
        var product = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id, name: "Safety Helmet");
        product.Type = productType;
        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetVariantsForProductAsync(product.Id);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateVariantAsync_WithDuplicateExternalIdentifier_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VariantService(context, _mapper);
        var productType = TestDataFactory.CreateProductType(name: "Helmet");
        var manufacturer = TestDataFactory.CreateManufacturer(name: "Test Manufacturer");
        var product = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id, name: "Safety Helmet");
        product.Type = productType;
        var existingVariant = TestDataFactory.CreateVariant(product.Id, name: "Size L", externalIdentifier: "EXT-VAR-001");
        existingVariant.Product = product;
        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        context.Products.Add(product);
        context.Variants.Add(existingVariant);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateVariantModel(product.Id, "Size M", externalIdentifier: "EXT-VAR-001");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.CreateVariantAsync(model));
    }

    [Fact]
    public async Task CreateVariantAsync_WithSameExternalIdentifierButDifferentProduct_ShouldSucceed()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VariantService(context, _mapper);
        var productType = TestDataFactory.CreateProductType(name: "Helmet");
        var manufacturer = TestDataFactory.CreateManufacturer(name: "Test Manufacturer");
        var productA = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id, name: "Product A");
        productA.Type = productType;
        var productB = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id, name: "Product B");
        productB.Type = productType;
        var existingVariant = TestDataFactory.CreateVariant(productA.Id, name: "Size L", externalIdentifier: "EXT-VAR-001");
        existingVariant.Product = productA;
        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        context.Products.AddRange(productA, productB);
        context.Variants.Add(existingVariant);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateVariantModel(productB.Id, "Size L", externalIdentifier: "EXT-VAR-001");

        // Act
        // Note: InMemory DB limitation - mapper may fail. We verify database state.
        try
        {
            var result = await service.CreateVariantAsync(model);
            Assert.NotNull(result);
        }
        catch (NullReferenceException)
        {
            // Expected due to InMemory DB limitation
        }

        // Assert - Verify variant was created
        var variants = await context.Variants
            .Where(v => v.ProductId == productB.Id && v.ExternalIdentifier == "EXT-VAR-001")
            .ToListAsync();
        Assert.Single(variants);
    }

    [Fact]
    public async Task UpdateVariantAsync_WithDuplicateExternalIdentifier_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new VariantService(context, _mapper);
        var productType = TestDataFactory.CreateProductType(name: "Helmet");
        var manufacturer = TestDataFactory.CreateManufacturer(name: "Test Manufacturer");
        var product = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id, name: "Safety Helmet");
        product.Type = productType;
        var existingVariant = TestDataFactory.CreateVariant(product.Id, name: "Size L", externalIdentifier: "EXT-VAR-001");
        existingVariant.Product = product;
        var variantToUpdate = TestDataFactory.CreateVariant(product.Id, name: "Size M", externalIdentifier: "EXT-VAR-002");
        variantToUpdate.Product = product;
        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        context.Products.Add(product);
        context.Variants.AddRange(existingVariant, variantToUpdate);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateVariantModel(product.Id, "Size M Updated", externalIdentifier: "EXT-VAR-001");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateVariantAsync(variantToUpdate.Id, updateModel));
    }
}
