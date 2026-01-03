using FireInvent.Database;
using FireInvent.Database.Extensions;
using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Extensions;
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

        if (!string.IsNullOrWhiteSpace(model.Identifier))
        {
            var exists = await context.Items
                .AnyAsync(c => c.Identifier == model.Identifier);

            if (exists)
                throw new ConflictException($"Item with identifier '{model.Identifier}' already exists.");
        }

        var item = mapper.MapCreateOrUpdateItemModelToItem(model);

        await context.Items.AddAsync(item);
        await context.SaveChangesAsync();

        item = await context.Items
            .AsNoTracking()
            .SingleAsync(i => i.Id == item.Id);

        return mapper.MapItemToItemModel(item);
    }

    public async Task<PagedResult<ItemModel>> GetAllItemsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var query = context.Items
            .OrderBy(i => i.PurchaseDate)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectItemsToItemModels(query);

        return await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);
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

        if (!string.IsNullOrWhiteSpace(model.Identifier))
        {
            var exists = await context.Items
                .AnyAsync(c => c.Identifier == model.Identifier && c.Id != id);

            if (exists)
                throw new ConflictException($"Item with identifier '{model.Identifier}' already exists.");
        }

        mapper.MapCreateOrUpdateItemModelToItem(model, item);

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

    public async Task<PagedResult<ItemModel>> GetItemsForVariantAsync(Guid variantId, PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var variantExists = await context.Variants.AnyAsync(v => v.Id == variantId);
        if (!variantExists)
            throw new NotFoundException($"Variant with ID {variantId} not found.");

        var query = context.Items
            .Where(i => i.VariantId == variantId)
            .OrderBy(i => i.PurchaseDate)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectItemsToItemModels(query);

        return await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);
    }
}