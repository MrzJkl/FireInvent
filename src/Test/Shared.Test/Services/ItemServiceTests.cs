using FireInvent.Contract;
using FireInvent.Database.Models;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Services;
using Microsoft.EntityFrameworkCore;

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

    private static Item CreateItemWithVariant(Variant variant, ItemCondition condition = ItemCondition.New, DateTimeOffset? purchaseDate = null, string? identifier = null, StorageLocation? storageLocation = null)
    {
        var item = TestDataFactory.CreateItem(variant.Id, condition: condition, purchaseDate: purchaseDate, identifier: identifier, storageLocationId: storageLocation?.Id);
        item.Variant = variant;
        item.StorageLocation = storageLocation;
        return item;
    }

    [Fact]
    public async Task CreateItemAsync_WithNonExistingVariant_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ItemService(context, _mapper);
        var model = TestDataFactory.CreateItemModel(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateItemAsync(model));
    }

    [Fact]
    public async Task CreateItemAsync_WithNonExistingStorageLocation_ShouldThrowBadRequestException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ItemService(context, _mapper);
        var (_, _, variant, _) = await SetupBasicDataAsync(context);

        var model = TestDataFactory.CreateItemModel(variant.Id, storageLocationId: Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateItemAsync(model));
    }

    [Fact]
    public async Task CreateItemAsync_WithDuplicateIdentifier_ShouldThrowConflictException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ItemService(context, _mapper);
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
        var service = new ItemService(context, _mapper);

        // Act
        var result = await service.GetAllItemsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetItemByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ItemService(context, _mapper);

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
        var service = new ItemService(context, _mapper);
        var (_, _, variant, storageLocation) = await SetupBasicDataAsync(context);

        var item = CreateItemWithVariant(variant, condition: ItemCondition.New, identifier: "ITEM-001");
        context.Items.Add(item);
        await context.SaveChangesAsync();

        var updateModel = TestDataFactory.CreateItemModel(variant.Id, ItemCondition.Used, identifier: "ITEM-001-UPDATED", storageLocationId: storageLocation.Id);

        // Act
        var result = await service.UpdateItemAsync(item.Id, updateModel);

        // Assert
        Assert.True(result);
        var updated = await context.Items.FindAsync(item.Id);
        Assert.NotNull(updated);
        Assert.Equal(ItemCondition.Used, updated.Condition);
        Assert.Equal("ITEM-001-UPDATED", updated.Identifier);
        Assert.Equal(storageLocation.Id, updated.StorageLocationId);
    }

    [Fact]
    public async Task UpdateItemAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ItemService(context, _mapper);
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
        var service = new ItemService(context, _mapper);
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
        var service = new ItemService(context, _mapper);
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
        var service = new ItemService(context, _mapper);
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
        var service = new ItemService(context, _mapper);

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
        var service = new ItemService(context, _mapper);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetItemsForVariantAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetItemsForStorageLocationAsync_WithNonExistingStorageLocation_ShouldThrowNotFoundException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ItemService(context, _mapper);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetItemsForStorageLocationAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetItemsAssignedToPersonAsync_WithNonExistingPerson_ShouldThrowNotFoundException()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ItemService(context, _mapper);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetItemsAssignedToPersonAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetItemsAssignedToPersonAsync_WithNoAssignments_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = TestHelper.GetTestDbContext();
        var service = new ItemService(context, _mapper);

        var person = TestDataFactory.CreatePerson(firstName: "John", lastName: "Doe");
        context.Persons.Add(person);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetItemsAssignedToPersonAsync(person.Id);

        // Assert
        Assert.Empty(result);
    }
}
