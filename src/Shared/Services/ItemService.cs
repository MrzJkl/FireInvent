using FireInvent.Database;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class ItemService(AppDbContext context, ItemMapper mapper) : IItemService
{
    public async Task<ItemModel> CreateItemAsync(CreateOrUpdateItemModel model)
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

        var item = mapper.MapCreateOrUpdateItemModelToItem(model);

        context.Items.Add(item);
        await context.SaveChangesAsync();

        var createdItem = await context.Items
            .AsNoTracking()
            .SingleAsync(i => i.Id == item.Id);

        return mapper.MapItemToItemModel(createdItem);
    }

    public async Task<List<ItemModel>> GetAllItemsAsync()
    {
        var items = await context.Items
            .AsNoTracking()
            .OrderBy(i => i.PurchaseDate)
            .ToListAsync();

        return mapper.MapItemsToItemModels(items);
    }

    public async Task<ItemModel?> GetItemByIdAsync(Guid id)
    {
        var item = await context.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);

        return item is null ? null : mapper.MapItemToItemModel(item);
    }

    public async Task<bool> UpdateItemAsync(Guid id, CreateOrUpdateItemModel model)
    {
        var item = await context.Items.FindAsync(id);
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
                .AnyAsync(c => c.Identifier == model.Identifier && c.Id != id);

            if (exists)
                throw new ConflictException($"Item with identifier '{model.Identifier}' already exists.");
        }

        mapper.MapCreateOrUpdateItemModelToItem(model, item, id);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteItemAsync(Guid id)
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

    public async Task<List<ItemModel>> GetItemsForStorageLocationAsync(Guid storageLocationId)
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

    public async Task<List<ItemModel>> GetItemsAssignedToPersonAsync(Guid personId)
    {
        var personExists = await context.Persons.AnyAsync(p => p.Id == personId);
        if (!personExists)
            throw new NotFoundException($"Person with ID {personId} not found.");

        var itemInfos = await context.ItemAssignmentHistories
            .Where(h => h.PersonId == personId)
            .GroupBy(h => h.ItemId)
            .Select(g => new { ItemId = g.Key, LastAssignedFrom = g.Max(h => h.AssignedFrom) })
            .OrderByDescending(x => x.LastAssignedFrom)
            .ToListAsync();

        var itemIds = itemInfos.Select(x => x.ItemId).ToList();
        if (itemIds.Count == 0)
            return [];

        var items = await context.Items
            .Where(i => itemIds.Contains(i.Id))
            .AsNoTracking()
            .ToListAsync();

        var itemsById = items.ToDictionary(i => i.Id);
        var orderedItems = itemInfos
            .Where(info => itemsById.ContainsKey(info.ItemId))
            .Select(info => itemsById[info.ItemId])
            .ToList();

        return mapper.MapItemsToItemModels(orderedItems);
    }
}