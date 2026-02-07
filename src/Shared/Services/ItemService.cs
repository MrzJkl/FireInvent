using FireInvent.Database;
using FireInvent.Database.Extensions;
using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Extensions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services.Telemetry;
using Microsoft.EntityFrameworkCore;
using FireInvent.Database.Models;

namespace FireInvent.Shared.Services;

public class ItemService(AppDbContext context, ItemMapper mapper, FireInventTelemetry telemetry) : IItemService
{
    public async Task<ItemModel> CreateItemAsync(CreateOrUpdateItemModel model, CancellationToken cancellationToken = default)
    {
        using var activity = telemetry.StartActivity("ItemService.CreateItem");
        activity?.SetTag("variant.id", model.VariantId);
        activity?.SetTag("item.identifier", model.Identifier);

        if (!await context.Variants.AnyAsync(v => v.Id == model.VariantId, cancellationToken))
            throw new BadRequestException("Variant not found.");

        if (!string.IsNullOrWhiteSpace(model.Identifier))
        {
            var exists = await context.Items
                .AnyAsync(c => c.Identifier == model.Identifier, cancellationToken);

            if (exists)
                throw new ConflictException($"Item with identifier '{model.Identifier}' already exists.");
        }

        var item = mapper.MapCreateOrUpdateItemModelToItem(model);

        await context.Items.AddAsync(item, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        item = await context.Items
            .AsNoTracking()
            .SingleAsync(i => i.Id == item.Id, cancellationToken);

        // Record telemetry
        telemetry.ItemsCreatedCounter.Add(1, 
            new KeyValuePair<string, object?>("variant.id", model.VariantId));

        activity?.SetTag("item.id", item.Id);

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

    public async Task<ItemModel?> GetItemByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await context.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        return item is null ? null : mapper.MapItemToItemModel(item);
    }

    public async Task<bool> UpdateItemAsync(Guid id, CreateOrUpdateItemModel model, CancellationToken cancellationToken = default)
    {
        var item = await context.Items.FindAsync(id, cancellationToken);
        if (item is null)
            return false;

        if (!await context.Variants.AnyAsync(v => v.Id == model.VariantId, cancellationToken))
            throw new BadRequestException("Variant not found.");

        if (!string.IsNullOrWhiteSpace(model.Identifier))
        {
            var exists = await context.Items
                .AnyAsync(c => c.Identifier == model.Identifier && c.Id != id, cancellationToken);

            if (exists)
                throw new ConflictException($"Item with identifier '{model.Identifier}' already exists.");
        }

        mapper.MapCreateOrUpdateItemModelToItem(model, item);

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteItemAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.TryDeleteEntityAsync(
            id,
            nameof(Item),
            context.Items,
            cancellationToken);
    }

    public async Task<PagedResult<ItemModel>> GetItemsForVariantAsync(Guid variantId, PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var variantExists = await context.Variants.AnyAsync(v => v.Id == variantId, cancellationToken);
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
