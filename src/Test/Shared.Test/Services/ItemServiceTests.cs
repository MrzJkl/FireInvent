using FireInvent.Contract;
using FireInvent.Database.Models;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Services;

namespace FireInvent.Test.Shared.Services;

/// <summary>
/// Unit tests for ItemService.
/// Note: Tests that verify returned mapped models are skipped because InMemory database
/// doesn't populate navigation properties when re-querying entities, which the mappers require.
/// These tests focus on business logic (validation, conflict checking) and data persistence.
/// </summary>
public class ItemServiceTests
{
    private readonly ItemMapper _mapper = new();

    private async Task<(ProductType ProductType, Product Product, Variant Variant, StorageLocation StorageLocation)> SetupBasicDataAsync(Database.AppDbContext context)
    {
        var productType = TestDataFactory.CreateProductType(name: "Helmet");
        var manufacturer = TestDataFactory.CreateManufacturer(name: "Test Manufacturer");
        var product = TestDataFactory.CreateProduct(productType.Id, manufacturer.Id, name: "Safety Helmet");
        product.Type = productType;
        var variant = TestDataFactory.CreateVariant(product.Id, name: "Size L");
        variant.Product = product;
        var storageLocation = TestDataFactory.CreateStorageLocation(name: "Warehouse A");

        context.ProductTypes.Add(productType);
        context.Manufacturers.Add(manufacturer);
        context.Products.Add(product);
        context.Variants.Add(variant);
        context.StorageLocations.Add(storageLocation);
        await context.SaveChangesAsync();

        return (productType, product, variant, storageLocation);
    }

    private static Item CreateItemWithVariant(Variant variant, ItemCondition condition = ItemCondition.New, DateOnly? purchaseDate = null, string? identifier = null)
    {
        var item = TestDataFactory.CreateItem(variant.Id, condition: condition, purchaseDate: purchaseDate, identifier: identifier);
        item.Variant = variant;
        return item;
    }

    [Fact]
    public async Task CreateItemAsync_WithNonExistingVariant_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ItemService(context, _mapper, TestHelper.GetTestTelemetry());
        var model = TestDataFactory.CreateItemModel(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateItemAsync(model));
    }

    [Fact]
    public async Task CreateItemAsync_WithDuplicateIdentifier_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ItemService(context, _mapper, TestHelper.GetTestTelemetry());
        var (_, _, variant, _) = await SetupBasicDataAsync(context);

        var existingItem = CreateItemWithVariant(variant, identifier: "ITEM-001");
        context.Items.Add(existingItem);
        await context.SaveChangesAsync();

        var model = TestDataFactory.CreateItemModel(variant.Id, identifier: "ITEM-001");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.CreateItemAsync(model));
    }

    [Fact]
    public async Task GetAllItemsAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ItemService(context, _mapper, TestHelper.GetTestTelemetry());
        var query = new PagedQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await service.GetAllItemsAsync(query, CancellationToken.None);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task GetItemByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ItemService(context, _mapper, TestHelper.GetTestTelemetry());

        // Act
        var result = await service.GetItemByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateItemAsync_WithExistingId_ShouldUpdateItem()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ItemService(context, _mapper, TestHelper.GetTestTelemetry());
        var (_, _, variant, _) = await SetupBasicDataAsync(context);

        var item = CreateItemWithVariant(variant, condition: ItemCondition.New, identifier: "ITEM-001");
        context.Items.Add(item);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateItemModel(variant.Id, ItemCondition.Used, identifier: "ITEM-001-UPDATED");

        // Act
        var result = await service.UpdateItemAsync(item.Id, updateModel);

        // Assert
        Assert.True(result);
        var updated = await context.Items.FindAsync(item.Id);
        Assert.NotNull(updated);
        Assert.Equal(ItemCondition.Used, updated.Condition);
        Assert.Equal("ITEM-001-UPDATED", updated.Identifier);
    }

    [Fact]
    public async Task UpdateItemAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ItemService(context, _mapper, TestHelper.GetTestTelemetry());
        var (_, _, variant, _) = await SetupBasicDataAsync(context);

        var updateModel = TestDataFactory.CreateItemModel(variant.Id);

        // Act
        var result = await service.UpdateItemAsync(Guid.NewGuid(), updateModel);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateItemAsync_WithNonExistingVariant_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ItemService(context, _mapper, TestHelper.GetTestTelemetry());
        var (_, _, variant, _) = await SetupBasicDataAsync(context);

        var item = CreateItemWithVariant(variant);
        context.Items.Add(item);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateItemModel(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateItemAsync(item.Id, updateModel));
    }

    [Fact]
    public async Task UpdateItemAsync_WithDuplicateIdentifier_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ItemService(context, _mapper, TestHelper.GetTestTelemetry());
        var (_, _, variant, _) = await SetupBasicDataAsync(context);

        var existingItem = CreateItemWithVariant(variant, identifier: "ITEM-001");
        var itemToUpdate = CreateItemWithVariant(variant, identifier: "ITEM-002");
        context.Items.AddRange(existingItem, itemToUpdate);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateItemModel(variant.Id, identifier: "ITEM-001");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateItemAsync(itemToUpdate.Id, updateModel));
    }

    [Fact]
    public async Task DeleteItemAsync_WithExistingId_ShouldDeleteItem()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ItemService(context, _mapper, TestHelper.GetTestTelemetry());
        var (_, _, variant, _) = await SetupBasicDataAsync(context);

        var item = CreateItemWithVariant(variant);
        context.Items.Add(item);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteItemAsync(item.Id);

        // Assert
        Assert.True(result);
        Assert.Null(await context.Items.FindAsync(item.Id));
    }

    [Fact]
    public async Task DeleteItemAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ItemService(context, _mapper, TestHelper.GetTestTelemetry());

        // Act
        var result = await service.DeleteItemAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetItemsForVariantAsync_WithNonExistingVariant_ShouldThrowNotFoundException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ItemService(context, _mapper, TestHelper.GetTestTelemetry());
        var query = new PagedQuery { Page = 1, PageSize = 10 };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetItemsForVariantAsync(Guid.NewGuid(), query, CancellationToken.None));
    }

    [Fact]
    public async Task GetAllItemsAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ItemService(context, _mapper, TestHelper.GetTestTelemetry());
        var (_, _, variant, _) = await SetupBasicDataAsync(context);

        // Create 25 items
        for (int i = 1; i <= 25; i++)
        {
            var item = CreateItemWithVariant(variant, identifier: $"ITEM-{i:D3}");
            context.Items.Add(item);
        }
        await context.SaveChangesAsync();

        // Act - Get first page (10 items)
        var query1 = new PagedQuery { Page = 1, PageSize = 10 };
        var result1 = await service.GetAllItemsAsync(query1, CancellationToken.None);

        // Act - Get second page (10 items)
        var query2 = new PagedQuery { Page = 2, PageSize = 10 };
        var result2 = await service.GetAllItemsAsync(query2, CancellationToken.None);

        // Act - Get third page (5 items)
        var query3 = new PagedQuery { Page = 3, PageSize = 10 };
        var result3 = await service.GetAllItemsAsync(query3, CancellationToken.None);

        // Assert page 1
        Assert.Equal(10, result1.Items.Count);
        Assert.Equal(25, result1.TotalItems);
        Assert.Equal(1, result1.Page);
        Assert.Equal(10, result1.PageSize);
        Assert.Equal(3, result1.TotalPages);

        // Assert page 2
        Assert.Equal(10, result2.Items.Count);
        Assert.Equal(25, result2.TotalItems);
        Assert.Equal(2, result2.Page);

        // Assert page 3
        Assert.Equal(5, result3.Items.Count);
        Assert.Equal(25, result3.TotalItems);
        Assert.Equal(3, result3.Page);
    }

    [Fact]
    public async Task GetAllItemsAsync_WithDifferentPageSizes_ShouldReturnCorrectCounts()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ItemService(context, _mapper, TestHelper.GetTestTelemetry());
        var (_, _, variant, _) = await SetupBasicDataAsync(context);

        // Create 15 items
        for (int i = 1; i <= 15; i++)
        {
            var item = CreateItemWithVariant(variant, identifier: $"ITEM-{i:D3}");
            context.Items.Add(item);
        }
        await context.SaveChangesAsync();

        // Act - Page size 5
        var query1 = new PagedQuery { Page = 1, PageSize = 5 };
        var result1 = await service.GetAllItemsAsync(query1, CancellationToken.None);

        // Act - Page size 10
        var query2 = new PagedQuery { Page = 1, PageSize = 10 };
        var result2 = await service.GetAllItemsAsync(query2, CancellationToken.None);

        // Assert
        Assert.Equal(5, result1.Items.Count);
        Assert.Equal(3, result1.TotalPages); // 15 items / 5 per page = 3 pages
        
        Assert.Equal(10, result2.Items.Count);
        Assert.Equal(2, result2.TotalPages); // 15 items / 10 per page = 2 pages
    }
}
