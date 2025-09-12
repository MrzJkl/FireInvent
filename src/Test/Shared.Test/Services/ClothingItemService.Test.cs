using AutoMapper;
using FireInvent.Contract;
using FireInvent.Database;
using FireInvent.Database.Models;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;

namespace FireInvent.Test.Shared.Services;

public class ClothingItemServiceTest
{
    private readonly IMapper _mapper;

    public ClothingItemServiceTest()
    {
        _mapper = TestHelper.GetMapper();
    }

    private static Variant CreateVariant(AppDbContext context)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            Type = ProductType.Jacket
        };
        context.ClothingProducts.Add(product);
        var variant = new Variant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Name = "Red",
            AdditionalSpecs = "XL, Reflective"
        };
        context.ClothingVariants.Add(variant);
        context.SaveChanges();
        return variant;
    }

    private static StorageLocation CreateStorageLocation(AppDbContext context)
    {
        var location = new StorageLocation
        {
            Id = Guid.NewGuid(),
            Name = "Main Storage",
        };
        context.StorageLocations.Add(location);
        context.SaveChanges();
        return location;
    }

    [Fact]
    public async Task CreateClothingItemAsync_ShouldCreateItem()
    {
        var context = TestHelper.GetTestDbContext();
        var variant = CreateVariant(context);
        var location = CreateStorageLocation(context);
        var service = new ClothingItemService(context, _mapper);

        var model = new CreateItemModel
        {
            VariantId = variant.Id,
            Identifier = "ITEM-001",
            StorageLocationId = location.Id,
            Condition = ItemCondition.New,
            PurchaseDate = new DateTime(2024, 1, 1),
            RetirementDate = new DateTime(2030, 1, 1)
        };

        var result = await service.CreateClothingItemAsync(model);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.VariantId, result.VariantId);
        Assert.Equal(model.Identifier, result.Identifier);
        Assert.Equal(model.StorageLocationId, result.StorageLocationId);
        Assert.Equal(model.Condition, result.Condition);
        Assert.Equal(model.PurchaseDate, result.PurchaseDate);
        Assert.Equal(model.RetirementDate, result.RetirementDate);

        var entity = await context.ClothingItems.FindAsync(result.Id);
        Assert.NotNull(entity);
        Assert.Equal(model.VariantId, entity!.VariantId);
        Assert.Equal(model.Identifier, entity.Identifier);
        Assert.Equal(model.StorageLocationId, entity.StorageLocationId);
        Assert.Equal(model.Condition, entity.Condition);
        Assert.Equal(model.PurchaseDate, entity.PurchaseDate);
        Assert.Equal(model.RetirementDate, entity.RetirementDate);
    }

    [Fact]
    public async Task CreateClothingItemAsync_ShouldThrowIfVariantNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new ClothingItemService(context, _mapper);

        var model = new CreateItemModel
        {
            VariantId = Guid.NewGuid(),
            Identifier = "ITEM-001",
            Condition = ItemCondition.New,
            PurchaseDate = DateTime.Now
        };

        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateClothingItemAsync(model));
    }

    [Fact]
    public async Task CreateClothingItemAsync_ShouldThrowIfStorageLocationNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var variant = CreateVariant(context);
        var service = new ClothingItemService(context, _mapper);

        var model = new CreateItemModel
        {
            VariantId = variant.Id,
            StorageLocationId = Guid.NewGuid(),
            Identifier = "ITEM-001",
            Condition = ItemCondition.New,
            PurchaseDate = DateTime.Now
        };

        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateClothingItemAsync(model));
    }

    [Fact]
    public async Task CreateClothingItemAsync_ShouldThrowIfIdentifierExists()
    {
        var context = TestHelper.GetTestDbContext();
        var variant = CreateVariant(context);
        var location = CreateStorageLocation(context);
        context.ClothingItems.Add(new Item
        {
            Id = Guid.NewGuid(),
            VariantId = variant.Id,
            Identifier = "ITEM-001",
            StorageLocationId = location.Id,
            Condition = ItemCondition.New,
            PurchaseDate = DateTime.Now
        });
        context.SaveChanges();

        var service = new ClothingItemService(context, _mapper);

        var model = new CreateItemModel
        {
            VariantId = variant.Id,
            Identifier = "ITEM-001",
            StorageLocationId = location.Id,
            Condition = ItemCondition.New,
            PurchaseDate = DateTime.Now
        };

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateClothingItemAsync(model));
    }

    [Fact]
    public async Task GetAllClothingItemsAsync_ShouldReturnAllItems()
    {
        var context = TestHelper.GetTestDbContext();
        var variant = CreateVariant(context);
        var location = CreateStorageLocation(context);
        var item1 = new Item
        {
            Id = Guid.NewGuid(),
            VariantId = variant.Id,
            Identifier = "ITEM-001",
            StorageLocationId = location.Id,
            Condition = ItemCondition.New,
            PurchaseDate = new DateTime(2024, 1, 1),
            RetirementDate = new DateTime(2030, 1, 1)
        };
        var item2 = new Item
        {
            Id = Guid.NewGuid(),
            VariantId = variant.Id,
            Identifier = "ITEM-002",
            StorageLocationId = location.Id,
            Condition = ItemCondition.Used,
            PurchaseDate = new DateTime(2023, 1, 1),
            RetirementDate = null
        };
        context.ClothingItems.Add(item1);
        context.ClothingItems.Add(item2);
        context.SaveChanges();

        var service = new ClothingItemService(context, _mapper);

        var result = await service.GetAllClothingItemsAsync();

        Assert.Equal(2, result.Count);
        var first = result.FirstOrDefault(i => i.Identifier == "ITEM-001");
        var second = result.FirstOrDefault(i => i.Identifier == "ITEM-002");

        Assert.NotNull(first);
        Assert.Equal(item1.Id, first!.Id);
        Assert.Equal(item1.VariantId, first.VariantId);
        Assert.Equal(item1.StorageLocationId, first.StorageLocationId);
        Assert.Equal(item1.Condition, first.Condition);
        Assert.Equal(item1.PurchaseDate, first.PurchaseDate);
        Assert.Equal(item1.RetirementDate, first.RetirementDate);

        Assert.NotNull(second);
        Assert.Equal(item2.Id, second!.Id);
        Assert.Equal(item2.VariantId, second.VariantId);
        Assert.Equal(item2.StorageLocationId, second.StorageLocationId);
        Assert.Equal(item2.Condition, second.Condition);
        Assert.Equal(item2.PurchaseDate, second.PurchaseDate);
        Assert.Equal(item2.RetirementDate, second.RetirementDate);
    }

    [Fact]
    public async Task GetClothingItemByIdAsync_ShouldReturnItem()
    {
        var context = TestHelper.GetTestDbContext();
        var variant = CreateVariant(context);
        var location = CreateStorageLocation(context);
        var item = new Item
        {
            Id = Guid.NewGuid(),
            VariantId = variant.Id,
            Identifier = "ITEM-001",
            StorageLocationId = location.Id,
            Condition = ItemCondition.New,
            PurchaseDate = new DateTime(2024, 1, 1),
            RetirementDate = new DateTime(2030, 1, 1)
        };
        context.ClothingItems.Add(item);
        context.SaveChanges();

        var service = new ClothingItemService(context, _mapper);

        var result = await service.GetClothingItemByIdAsync(item.Id);

        Assert.NotNull(result);
        Assert.Equal(item.Id, result!.Id);
        Assert.Equal(item.VariantId, result.VariantId);
        Assert.Equal(item.Identifier, result.Identifier);
        Assert.Equal(item.StorageLocationId, result.StorageLocationId);
        Assert.Equal(item.Condition, result.Condition);
        Assert.Equal(item.PurchaseDate, result.PurchaseDate);
        Assert.Equal(item.RetirementDate, result.RetirementDate);
    }

    [Fact]
    public async Task GetClothingItemByIdAsync_ShouldReturnNullIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new ClothingItemService(context, _mapper);

        var result = await service.GetClothingItemByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateClothingItemAsync_ShouldUpdateItem()
    {
        var context = TestHelper.GetTestDbContext();
        var variant = CreateVariant(context);
        var location = CreateStorageLocation(context);
        var item = new Item
        {
            Id = Guid.NewGuid(),
            VariantId = variant.Id,
            Identifier = "ITEM-001",
            StorageLocationId = location.Id,
            Condition = ItemCondition.New,
            PurchaseDate = new DateTime(2024, 1, 1),
            RetirementDate = new DateTime(2030, 1, 1)
        };
        context.ClothingItems.Add(item);
        context.SaveChanges();

        var service = new ClothingItemService(context, _mapper);

        var model = new ItemModel
        {
            Id = item.Id,
            VariantId = variant.Id,
            Identifier = "ITEM-001-UPDATED",
            StorageLocationId = location.Id,
            Condition = ItemCondition.Damaged,
            PurchaseDate = new DateTime(2025, 1, 1),
            RetirementDate = new DateTime(2031, 1, 1)
        };

        var result = await service.UpdateClothingItemAsync(model);

        Assert.True(result);
        var updated = await context.ClothingItems.FindAsync(item.Id);
        Assert.NotNull(updated);
        Assert.Equal(model.Identifier, updated!.Identifier);
        Assert.Equal(model.Condition, updated.Condition);
        Assert.Equal(model.PurchaseDate, updated.PurchaseDate);
        Assert.Equal(model.RetirementDate, updated.RetirementDate);
    }

    [Fact]
    public async Task UpdateClothingItemAsync_ShouldReturnFalseIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var variant = CreateVariant(context);
        var location = CreateStorageLocation(context);
        var service = new ClothingItemService(context, _mapper);

        var model = new ItemModel
        {
            Id = Guid.NewGuid(),
            VariantId = variant.Id,
            Identifier = "ITEM-001",
            StorageLocationId = location.Id,
            Condition = ItemCondition.New,
            PurchaseDate = new DateTime(2024, 1, 1),
            RetirementDate = new DateTime(2030, 1, 1)
        };

        var result = await service.UpdateClothingItemAsync(model);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateClothingItemAsync_ShouldThrowIfVariantNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var variant = CreateVariant(context);
        var location = CreateStorageLocation(context);
        var item = new Item
        {
            Id = Guid.NewGuid(),
            VariantId = variant.Id,
            Identifier = "ITEM-001",
            StorageLocationId = location.Id,
            Condition = ItemCondition.New,
            PurchaseDate = new DateTime(2024, 1, 1),
            RetirementDate = new DateTime(2030, 1, 1)
        };
        context.ClothingItems.Add(item);
        context.SaveChanges();

        var service = new ClothingItemService(context, _mapper);

        var model = new ItemModel
        {
            Id = item.Id,
            VariantId = Guid.NewGuid(), // not existing
            Identifier = "ITEM-001-UPDATED",
            StorageLocationId = location.Id,
            Condition = ItemCondition.Damaged,
            PurchaseDate = new DateTime(2025, 1, 1),
            RetirementDate = new DateTime(2031, 1, 1)
        };

        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateClothingItemAsync(model));
    }

    [Fact]
    public async Task UpdateClothingItemAsync_ShouldThrowIfStorageLocationNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var variant = CreateVariant(context);
        var location = CreateStorageLocation(context);
        var item = new Item
        {
            Id = Guid.NewGuid(),
            VariantId = variant.Id,
            Identifier = "ITEM-001",
            StorageLocationId = location.Id,
            Condition = ItemCondition.New,
            PurchaseDate = new DateTime(2024, 1, 1),
            RetirementDate = new DateTime(2030, 1, 1)
        };
        context.ClothingItems.Add(item);
        context.SaveChanges();

        var service = new ClothingItemService(context, _mapper);

        var model = new ItemModel
        {
            Id = item.Id,
            VariantId = variant.Id,
            Identifier = "ITEM-001-UPDATED",
            StorageLocationId = Guid.NewGuid(), // not existing
            Condition = ItemCondition.Damaged,
            PurchaseDate = new DateTime(2025, 1, 1),
            RetirementDate = new DateTime(2031, 1, 1)
        };

        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateClothingItemAsync(model));
    }

    [Fact]
    public async Task UpdateClothingItemAsync_ShouldThrowIfIdentifierExists()
    {
        var context = TestHelper.GetTestDbContext();
        var variant = CreateVariant(context);
        var location = CreateStorageLocation(context);
        var item1 = new Item
        {
            Id = Guid.NewGuid(),
            VariantId = variant.Id,
            Identifier = "ITEM-001",
            StorageLocationId = location.Id,
            Condition = ItemCondition.New,
            PurchaseDate = new DateTime(2024, 1, 1),
            RetirementDate = new DateTime(2030, 1, 1)
        };
        var item2 = new Item
        {
            Id = Guid.NewGuid(),
            VariantId = variant.Id,
            Identifier = "ITEM-002",
            StorageLocationId = location.Id,
            Condition = ItemCondition.Used,
            PurchaseDate = new DateTime(2023, 1, 1),
            RetirementDate = null
        };
        context.ClothingItems.Add(item1);
        context.ClothingItems.Add(item2);
        context.SaveChanges();

        var service = new ClothingItemService(context, _mapper);

        var model = new ItemModel
        {
            Id = item2.Id,
            VariantId = variant.Id,
            Identifier = "ITEM-001", // duplicate
            StorageLocationId = location.Id,
            Condition = ItemCondition.Used,
            PurchaseDate = new DateTime(2023, 1, 1),
            RetirementDate = null
        };

        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateClothingItemAsync(model));
    }

    [Fact]
    public async Task DeleteClothingItemAsync_ShouldDeleteItem()
    {
        var context = TestHelper.GetTestDbContext();
        var variant = CreateVariant(context);
        var location = CreateStorageLocation(context);
        var item = new Item
        {
            Id = Guid.NewGuid(),
            VariantId = variant.Id,
            Identifier = "ITEM-001",
            StorageLocationId = location.Id,
            Condition = ItemCondition.New,
            PurchaseDate = new DateTime(2024, 1, 1),
            RetirementDate = new DateTime(2030, 1, 1)
        };
        context.ClothingItems.Add(item);
        context.SaveChanges();

        var service = new ClothingItemService(context, _mapper);

        var result = await service.DeleteClothingItemAsync(item.Id);

        Assert.True(result);
        Assert.False(context.ClothingItems.Any());
    }

    [Fact]
    public async Task DeleteClothingItemAsync_ShouldReturnFalseIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new ClothingItemService(context, _mapper);

        var result = await service.DeleteClothingItemAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task GetItemsForVariantAsync_ShouldReturnItemsForVariant()
    {
        var context = TestHelper.GetTestDbContext();
        var variant = CreateVariant(context);
        var otherVariant = CreateVariant(context);
        var location = CreateStorageLocation(context);
        var item1 = new Item
        {
            Id = Guid.NewGuid(),
            VariantId = variant.Id,
            Identifier = "ITEM-001",
            StorageLocationId = location.Id,
            Condition = ItemCondition.New,
            PurchaseDate = new DateTime(2024, 1, 1),
            RetirementDate = new DateTime(2030, 1, 1)
        };
        var item2 = new Item
        {
            Id = Guid.NewGuid(),
            VariantId = variant.Id,
            Identifier = "ITEM-002",
            StorageLocationId = location.Id,
            Condition = ItemCondition.Used,
            PurchaseDate = new DateTime(2023, 1, 1),
            RetirementDate = null
        };
        var itemOther = new Item
        {
            Id = Guid.NewGuid(),
            VariantId = otherVariant.Id,
            Identifier = "ITEM-003",
            StorageLocationId = location.Id,
            Condition = ItemCondition.Used,
            PurchaseDate = new DateTime(2022, 1, 1),
            RetirementDate = null
        };
        context.ClothingItems.Add(item1);
        context.ClothingItems.Add(item2);
        context.ClothingItems.Add(itemOther);
        context.SaveChanges();

        var service = new ClothingItemService(context, _mapper);

        var result = await service.GetItemsForVariantAsync(variant.Id);

        Assert.Equal(2, result.Count);
        var first = result.FirstOrDefault(i => i.Identifier == "ITEM-001");
        var second = result.FirstOrDefault(i => i.Identifier == "ITEM-002");

        Assert.NotNull(first);
        Assert.Equal(item1.Id, first!.Id);
        Assert.Equal(item1.VariantId, first.VariantId);
        Assert.Equal(item1.StorageLocationId, first.StorageLocationId);
        Assert.Equal(item1.Condition, first.Condition);
        Assert.Equal(item1.PurchaseDate, first.PurchaseDate);
        Assert.Equal(item1.RetirementDate, first.RetirementDate);

        Assert.NotNull(second);
        Assert.Equal(item2.Id, second!.Id);
        Assert.Equal(item2.VariantId, second.VariantId);
        Assert.Equal(item2.StorageLocationId, second.StorageLocationId);
        Assert.Equal(item2.Condition, second.Condition);
        Assert.Equal(item2.PurchaseDate, second.PurchaseDate);
        Assert.Equal(item2.RetirementDate, second.RetirementDate);
    }

    [Fact]
    public async Task GetItemsForVariantAsync_ShouldThrowIfVariantNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new ClothingItemService(context, _mapper);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetItemsForVariantAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetClothingItemsForStorageLocationAsync_ShouldReturnItemsForLocation()
    {
        var context = TestHelper.GetTestDbContext();
        var variant = CreateVariant(context);
        var location = CreateStorageLocation(context);
        var otherLocation = CreateStorageLocation(context);
        var item1 = new Item
        {
            Id = Guid.NewGuid(),
            VariantId = variant.Id,
            Identifier = "ITEM-001",
            StorageLocationId = location.Id,
            Condition = ItemCondition.New,
            PurchaseDate = new DateTime(2024, 1, 1),
            RetirementDate = new DateTime(2030, 1, 1)
        };
        var item2 = new Item
        {
            Id = Guid.NewGuid(),
            VariantId = variant.Id,
            Identifier = "ITEM-002",
            StorageLocationId = location.Id,
            Condition = ItemCondition.Used,
            PurchaseDate = new DateTime(2023, 1, 1),
            RetirementDate = null
        };
        var itemOther = new Item
        {
            Id = Guid.NewGuid(),
            VariantId = variant.Id,
            Identifier = "ITEM-003",
            StorageLocationId = otherLocation.Id,
            Condition = ItemCondition.Used,
            PurchaseDate = new DateTime(2022, 1, 1),
            RetirementDate = null
        };
        context.ClothingItems.Add(item1);
        context.ClothingItems.Add(item2);
        context.ClothingItems.Add(itemOther);
        context.SaveChanges();

        var service = new ClothingItemService(context, _mapper);

        var result = await service.GetClothingItemsForStorageLocationAsync(location.Id);

        Assert.Equal(2, result.Count);
        var first = result.FirstOrDefault(i => i.Identifier == "ITEM-001");
        var second = result.FirstOrDefault(i => i.Identifier == "ITEM-002");

        Assert.NotNull(first);
        Assert.Equal(item1.Id, first!.Id);
        Assert.Equal(item1.VariantId, first.VariantId);
        Assert.Equal(item1.StorageLocationId, first.StorageLocationId);
        Assert.Equal(item1.Condition, first.Condition);
        Assert.Equal(item1.PurchaseDate, first.PurchaseDate);
        Assert.Equal(item1.RetirementDate, first.RetirementDate);

        Assert.NotNull(second);
        Assert.Equal(item2.Id, second!.Id);
        Assert.Equal(item2.VariantId, second.VariantId);
        Assert.Equal(item2.StorageLocationId, second.StorageLocationId);
        Assert.Equal(item2.Condition, second.Condition);
        Assert.Equal(item2.PurchaseDate, second.PurchaseDate);
        Assert.Equal(item2.RetirementDate, second.RetirementDate);
    }

    [Fact]
    public async Task GetClothingItemsForStorageLocationAsync_ShouldThrowIfLocationNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new ClothingItemService(context, _mapper);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetClothingItemsForStorageLocationAsync(Guid.NewGuid()));
    }
}