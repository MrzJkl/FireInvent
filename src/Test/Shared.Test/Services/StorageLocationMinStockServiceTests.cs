using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Services;

namespace FireInvent.Test.Shared.Services;

/// <summary>
/// Unit tests for StorageLocationService min-stock CRUD and warning detection.
/// </summary>
public class StorageLocationMinStockServiceTests
{
    private readonly StorageLocationMapper _mapper = new();
    private readonly StorageLocationMinStockMapper _minStockMapper = new();

    private StorageLocationService CreateService(FireInvent.Database.AppDbContext context)
        => new(context, _mapper, _minStockMapper);

    // ---- SetMinStockAsync ----

    [Fact]
    public async Task SetMinStockAsync_WithNewVariant_ShouldCreateMinStock()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = CreateService(context);

        var location = TestDataFactory.CreateStorageLocation(name: "Kleiderkammer");
        var productType = TestDataFactory.CreateProductType();
        var manufacturer = TestDataFactory.CreateManufacturer();
        context.StorageLocations.Add(location);
        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        await context.SaveChangesAsync();

        var product = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id, name: "Feuerschutzhose");
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var variant = TestDataFactory.CreateVariant(product.Id, name: "Größe M");
        context.Variants.Add(variant);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateStorageLocationMinStockModel(variant.Id, minStock: 5);

        // Act
        var result = await service.SetMinStockAsync(location.Id, model);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(location.Id, result.StorageLocationId);
        Assert.Equal(variant.Id, result.VariantId);
        Assert.Equal("Größe M", result.VariantName);
        Assert.Equal("Feuerschutzhose", result.ProductName);
        Assert.Equal(5, result.MinStock);
        Assert.Equal(0, result.CurrentStock);
        Assert.True(result.IsBelow);
    }

    [Fact]
    public async Task SetMinStockAsync_WithExistingVariant_ShouldUpdateMinStock()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = CreateService(context);

        var location = TestDataFactory.CreateStorageLocation();
        var productType = TestDataFactory.CreateProductType();
        var manufacturer = TestDataFactory.CreateManufacturer();
        context.StorageLocations.Add(location);
        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        await context.SaveChangesAsync();

        var product = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var variant = TestDataFactory.CreateVariant(product.Id);
        context.Variants.Add(variant);
        await context.SaveChangesAsync();

        var existing = TestDataFactory.CreateStorageLocationMinStock(location.Id, variant.Id, minStock: 3);
        context.StorageLocationMinStocks.Add(existing);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateStorageLocationMinStockModel(variant.Id, minStock: 10);

        // Act
        var result = await service.SetMinStockAsync(location.Id, model);

        // Assert
        Assert.Equal(10, result.MinStock);
        // verify only one row exists
        Assert.Single(context.StorageLocationMinStocks);
    }

    [Fact]
    public async Task SetMinStockAsync_WithNonExistingLocation_ShouldThrowNotFoundException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = CreateService(context);
        var model = TestDataFactory.CreateStorageLocationMinStockModel(Guid.NewGuid(), minStock: 5);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.SetMinStockAsync(Guid.NewGuid(), model));
    }

    [Fact]
    public async Task SetMinStockAsync_WithNonExistingVariant_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = CreateService(context);

        var location = TestDataFactory.CreateStorageLocation();
        context.StorageLocations.Add(location);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateStorageLocationMinStockModel(Guid.NewGuid(), minStock: 5);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.SetMinStockAsync(location.Id, model));
    }

    // ---- GetMinStocksForStorageLocationAsync ----

    [Fact]
    public async Task GetMinStocksForStorageLocationAsync_ShouldReturnAllMinStocksWithCurrentCount()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = CreateService(context);

        var location = TestDataFactory.CreateStorageLocation();
        var productType = TestDataFactory.CreateProductType();
        var manufacturer = TestDataFactory.CreateManufacturer();
        context.StorageLocations.Add(location);
        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        await context.SaveChangesAsync();

        var product = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var variantM = TestDataFactory.CreateVariant(product.Id, name: "M");
        var variantL = TestDataFactory.CreateVariant(product.Id, name: "L");
        context.Variants.AddRange(variantM, variantL);
        await context.SaveChangesAsync();

        var minStockM = TestDataFactory.CreateStorageLocationMinStock(location.Id, variantM.Id, minStock: 5);
        var minStockL = TestDataFactory.CreateStorageLocationMinStock(location.Id, variantL.Id, minStock: 2);
        context.StorageLocationMinStocks.AddRange(minStockM, minStockL);

        // Add one active item of variantM in this location
        var item = TestDataFactory.CreateItem(variantM.Id);
        context.Items.Add(item);
        await context.SaveChangesAsync();

        var assignment = TestDataFactory.CreateAssignment(
            item.Id,
            storageLocationId: location.Id,
            assignedFrom: DateOnly.FromDateTime(DateTime.UtcNow));
        context.ItemAssignmentHistories.Add(assignment);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetMinStocksForStorageLocationAsync(location.Id);

        // Assert
        Assert.Equal(2, result.Count);
        var stockM = result.Single(r => r.VariantId == variantM.Id);
        var stockL = result.Single(r => r.VariantId == variantL.Id);

        Assert.Equal(5, stockM.MinStock);
        Assert.Equal(1, stockM.CurrentStock);
        Assert.True(stockM.IsBelow); // 1 < 5

        Assert.Equal(2, stockL.MinStock);
        Assert.Equal(0, stockL.CurrentStock);
        Assert.True(stockL.IsBelow); // 0 < 2
    }

    [Fact]
    public async Task GetMinStocksForStorageLocationAsync_WithNonExistingLocation_ShouldThrowNotFoundException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = CreateService(context);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetMinStocksForStorageLocationAsync(Guid.NewGuid()));
    }

    // ---- DeleteMinStockAsync ----

    [Fact]
    public async Task DeleteMinStockAsync_WithExistingEntry_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = CreateService(context);

        var location = TestDataFactory.CreateStorageLocation();
        context.StorageLocations.Add(location);
        await context.SaveChangesAsync();

        var productType = TestDataFactory.CreateProductType();
        var manufacturer = TestDataFactory.CreateManufacturer();
        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        await context.SaveChangesAsync();

        var product = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var variant = TestDataFactory.CreateVariant(product.Id);
        context.Variants.Add(variant);
        await context.SaveChangesAsync();

        var minStock = TestDataFactory.CreateStorageLocationMinStock(location.Id, variant.Id);
        context.StorageLocationMinStocks.Add(minStock);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteMinStockAsync(location.Id, variant.Id);

        // Assert
        Assert.True(result);
        Assert.Empty(context.StorageLocationMinStocks);
    }

    [Fact]
    public async Task DeleteMinStockAsync_WithNonExistingEntry_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.DeleteMinStockAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    // ---- HasStockWarning ----

    [Fact]
    public async Task GetAllStorageLocationsAsync_WhenStockBelowMinimum_ShouldSetHasStockWarningTrue()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = CreateService(context);

        var location = TestDataFactory.CreateStorageLocation(name: "Kleiderkammer");
        context.StorageLocations.Add(location);
        await context.SaveChangesAsync();

        var productType = TestDataFactory.CreateProductType();
        var manufacturer = TestDataFactory.CreateManufacturer();
        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        await context.SaveChangesAsync();

        var product = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var variant = TestDataFactory.CreateVariant(product.Id, name: "M");
        context.Variants.Add(variant);
        await context.SaveChangesAsync();

        // Set min stock = 3 but have 0 items → warning expected
        var minStock = TestDataFactory.CreateStorageLocationMinStock(location.Id, variant.Id, minStock: 3);
        context.StorageLocationMinStocks.Add(minStock);
        await context.SaveChangesAsync();

        var query = new FireInvent.Contract.PagedQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetAllStorageLocationsAsync(query, CancellationToken.None);

        // Assert
        Assert.Single(result.Items);
        Assert.True(result.Items[0].HasStockWarning);
    }

    [Fact]
    public async Task GetAllStorageLocationsAsync_WhenStockAtOrAboveMinimum_ShouldSetHasStockWarningFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = CreateService(context);

        var location = TestDataFactory.CreateStorageLocation(name: "Kleiderkammer");
        context.StorageLocations.Add(location);
        await context.SaveChangesAsync();

        var productType = TestDataFactory.CreateProductType();
        var manufacturer = TestDataFactory.CreateManufacturer();
        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        await context.SaveChangesAsync();

        var product = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var variant = TestDataFactory.CreateVariant(product.Id, name: "M");
        context.Variants.Add(variant);
        await context.SaveChangesAsync();

        // Set min stock = 1, add 1 item → no warning
        var minStock = TestDataFactory.CreateStorageLocationMinStock(location.Id, variant.Id, minStock: 1);
        context.StorageLocationMinStocks.Add(minStock);

        var item = TestDataFactory.CreateItem(variant.Id);
        context.Items.Add(item);
        await context.SaveChangesAsync();

        var assignment = TestDataFactory.CreateAssignment(
            item.Id,
            storageLocationId: location.Id,
            assignedFrom: DateOnly.FromDateTime(DateTime.UtcNow));
        context.ItemAssignmentHistories.Add(assignment);
        await context.SaveChangesAsync();

        var query = new FireInvent.Contract.PagedQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetAllStorageLocationsAsync(query, CancellationToken.None);

        // Assert
        Assert.Single(result.Items);
        Assert.False(result.Items[0].HasStockWarning);
    }

    [Fact]
    public async Task GetStorageLocationByIdAsync_WhenStockBelowMinimum_ShouldSetHasStockWarningTrue()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = CreateService(context);

        var location = TestDataFactory.CreateStorageLocation(name: "Kleiderkammer");
        context.StorageLocations.Add(location);
        await context.SaveChangesAsync();

        var productType = TestDataFactory.CreateProductType();
        var manufacturer = TestDataFactory.CreateManufacturer();
        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        await context.SaveChangesAsync();

        var product = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var variant = TestDataFactory.CreateVariant(product.Id, name: "M");
        context.Variants.Add(variant);
        await context.SaveChangesAsync();

        // min stock = 5, no items in location → warning
        var minStock = TestDataFactory.CreateStorageLocationMinStock(location.Id, variant.Id, minStock: 5);
        context.StorageLocationMinStocks.Add(minStock);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetStorageLocationByIdAsync(location.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.HasStockWarning);
    }

    [Fact]
    public async Task GetStorageLocationByIdAsync_WithNoMinStocksConfigured_ShouldNotHaveWarning()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = CreateService(context);

        var location = TestDataFactory.CreateStorageLocation(name: "Lager ohne Mindestbestand");
        context.StorageLocations.Add(location);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetStorageLocationByIdAsync(location.Id);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.HasStockWarning);
    }

    [Fact]
    public async Task SetMinStockAsync_CurrentStockIsCorrectlyCountedForActiveAssignmentsOnly()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = CreateService(context);

        var location = TestDataFactory.CreateStorageLocation();
        context.StorageLocations.Add(location);
        await context.SaveChangesAsync();

        var productType = TestDataFactory.CreateProductType();
        var manufacturer = TestDataFactory.CreateManufacturer();
        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        await context.SaveChangesAsync();

        var product = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var variant = TestDataFactory.CreateVariant(product.Id);
        context.Variants.Add(variant);
        await context.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Active item assignment (no end date)
        var item1 = TestDataFactory.CreateItem(variant.Id);
        var item2 = TestDataFactory.CreateItem(variant.Id);
        context.Items.AddRange(item1, item2);
        await context.SaveChangesAsync();

        // item1 is currently in location (active, no end date)
        var activeAssignment = TestDataFactory.CreateAssignment(
            item1.Id,
            storageLocationId: location.Id,
            assignedFrom: today.AddDays(-10));

        // item2 was in location but left yesterday (expired assignment)
        var expiredAssignment = TestDataFactory.CreateAssignment(
            item2.Id,
            storageLocationId: location.Id,
            assignedFrom: today.AddDays(-20),
            assignedUntil: today.AddDays(-1));

        context.ItemAssignmentHistories.AddRange(activeAssignment, expiredAssignment);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateStorageLocationMinStockModel(variant.Id, minStock: 3);

        // Act
        var result = await service.SetMinStockAsync(location.Id, model);

        // Assert
        // Only item1 is currently in location (item2's assignment has expired)
        Assert.Equal(1, result.CurrentStock);
        Assert.True(result.IsBelow); // 1 < 3
    }
}
