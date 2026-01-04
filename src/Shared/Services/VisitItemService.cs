using FireInvent.Database;
using FireInvent.Database.Extensions;
using FireInvent.Contract;
using FireInvent.Shared.Extensions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class VisitItemService(AppDbContext context, VisitMapper mapper) : IVisitItemService
{
    public async Task<VisitItemModel?> GetVisitItemByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var visitItem = await context.VisitItems
            .AsNoTracking()
            .FirstOrDefaultAsync(vi => vi.Id == id, cancellationToken);

        return visitItem is null ? null : mapper.MapVisitItemToVisitItemModel(visitItem);
    }

    public async Task<PagedResult<VisitItemModel>> GetAllVisitItemsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var query = context.VisitItems
            .OrderBy(vi => vi.VisitId)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectVisitItemsToVisitItemModels(query);

        return await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<VisitItemModel>> GetVisitItemsByVisitIdAsync(Guid visitId, PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var query = context.VisitItems
            .Where(vi => vi.VisitId == visitId)
            .OrderBy(vi => vi.CreatedAt)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectVisitItemsToVisitItemModels(query);

        return await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);
    }

    public async Task<VisitItemModel> CreateVisitItemAsync(CreateOrUpdateVisitItemModel model, CancellationToken cancellationToken = default)
    {
        var visitItem = mapper.MapCreateOrUpdateVisitItemModelToVisitItem(model);

        await context.VisitItems.AddAsync(visitItem, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        visitItem = await context.VisitItems
            .AsNoTracking()
            .FirstAsync(vi => vi.Id == visitItem.Id, cancellationToken);

        return mapper.MapVisitItemToVisitItemModel(visitItem);
    }

    public async Task<bool> UpdateVisitItemAsync(Guid id, CreateOrUpdateVisitItemModel model, CancellationToken cancellationToken = default)
    {
        var entity = await context.VisitItems
            .FirstOrDefaultAsync(vi => vi.Id == id, cancellationToken);

        if (entity is null)
            return false;

        mapper.MapCreateOrUpdateVisitItemModelToVisitItem(model, entity);

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteVisitItemAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await context.VisitItems
            .FirstOrDefaultAsync(vi => vi.Id == id, cancellationToken);

        if (entity is null)
            return false;

        context.VisitItems.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
