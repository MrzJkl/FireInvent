using FireInvent.Database;
using FireInvent.Database.Models;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class ItemService(AppDbContext context, ItemMapper mapper) : IItemService
{
    public async Task<ItemModel> CreateClothingItemAsync(CreateItemModel model)
    {
        if (!await context.Variants.AnyAsync(v => v.Id == model.VariantId))
            throw new BadRequestException("Variant not found.");

        if (model.StorageLocationId.HasValue &&
            !await context.StorageLocations.AnyAsync(s => s.Id == model.StorageLocationId))
            throw new BadRequestException("StorageLocation not found.");

        if (!string.IsNullOrWhiteSpace(model.Identifier))
        {
            var exists = await context.Items
                .AnyAsync(c => c.Identifier == model.Identifier);

            if (exists)
                throw new ConflictException($"Item with identifier '{model.Identifier}' already exists.");
        }

        var item = mapper.MapCreateItemModelToItem(model);

        context.Items.Add(item);
        await context.SaveChangesAsync();

        return mapper.MapItemToItemModel(item);
    }

    public async Task<List<ItemModel>> GetAllClothingItemsAsync()
    {
        var items = await context.Items
            .AsNoTracking()
            .OrderBy(i => i.PurchaseDate)
            .ToListAsync();

        return mapper.MapItemsToItemModels(items);
    }

    public async Task<ItemModel?> GetClothingItemByIdAsync(Guid id)
    {
        var item = await context.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);

        return item is null ? null : mapper.MapItemToItemModel(item);
    }

    public async Task<bool> UpdateClothingItemAsync(ItemModel model)
    {
        var item = await context.Items.FindAsync(model.Id);
        if (item is null)
            return false;

        if (!await context.Variants.AnyAsync(v => v.Id == model.VariantId))
            throw new BadRequestException("Variant not found.");

        if (model.StorageLocationId.HasValue &&
            !await context.StorageLocations.AnyAsync(s => s.Id == model.StorageLocationId))
            throw new BadRequestException("StorageLocation not found.");

        if (!string.IsNullOrWhiteSpace(model.Identifier))
        {
            var exists = await context.Items
                .AnyAsync(c => c.Identifier == model.Identifier && c.Id != model.Id);

            if (exists)
                throw new ConflictException($"ClothingItem with identifier '{model.Identifier}' already exists.");
        }

        mapper.MapItemModelToItem(model, item);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteClothingItemAsync(Guid id)
    {
        var item = await context.Items.FindAsync(id);
        if (item is null)
            return false;

        context.Items.Remove(item);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ItemModel>> GetItemsForVariantAsync(Guid variantId)
    {
        var variantExists = await context.Variants.AnyAsync(v => v.Id == variantId);
        if (!variantExists)
            throw new NotFoundException($"Variant with ID {variantId} not found.");

        var items = await context.Items
            .Where(i => i.VariantId == variantId)
            .OrderBy(i => i.PurchaseDate)
            .AsNoTracking()
            .ToListAsync();

        return mapper.MapItemsToItemModels(items);
    }

    public async Task<List<ItemModel>> GetClothingItemsForStorageLocationAsync(Guid storageLocationId)
    {
        var locationExists = await context.StorageLocations.AnyAsync(s => s.Id == storageLocationId);
        if (!locationExists)
            throw new NotFoundException($"StorageLocation with ID {storageLocationId} not found.");

        var items = await context.Items
            .Where(i => i.StorageLocationId == storageLocationId)
            .OrderBy(i => i.PurchaseDate)
            .AsNoTracking()
            .ToListAsync();

        return mapper.MapItemsToItemModels(items);
    }
}