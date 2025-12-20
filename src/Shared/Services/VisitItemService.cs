using FireInvent.Database;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class VisitItemService(AppDbContext context, VisitMapper mapper) : IVisitItemService
{
    public async Task<VisitItemModel?> GetVisitItemByIdAsync(Guid id)
    {
        var visitItem = await context.VisitItems
            .AsNoTracking()
            .FirstOrDefaultAsync(vi => vi.Id == id);

        return visitItem is null ? null : mapper.MapVisitItemToVisitItemModel(visitItem);
    }

    public async Task<List<VisitItemModel>> GetAllVisitItemsAsync()
    {
        var visitItems = await context.VisitItems
            .AsNoTracking()
            .ToListAsync();

        return mapper.MapVisitItemsToVisitItemModels(visitItems);
    }

    public async Task<List<VisitItemModel>> GetVisitItemsByVisitIdAsync(Guid visitId)
    {
        var visitItems = await context.VisitItems
            .AsNoTracking()
            .Where(vi => vi.VisitId == visitId)
            .ToListAsync();

        return mapper.MapVisitItemsToVisitItemModels(visitItems);
    }

    public async Task<VisitItemModel> CreateVisitItemAsync(CreateOrUpdateVisitItemModel model)
    {
        var visitItem = mapper.MapCreateOrUpdateVisitItemModelToVisitItem(model);

        await context.VisitItems.AddAsync(visitItem);
        await context.SaveChangesAsync();

        visitItem = await context.VisitItems
            .AsNoTracking()
            .FirstAsync(vi => vi.Id == visitItem.Id);

        return mapper.MapVisitItemToVisitItemModel(visitItem);
    }

    public async Task<bool> UpdateVisitItemAsync(Guid id, CreateOrUpdateVisitItemModel model)
    {
        var entity = await context.VisitItems
            .FirstOrDefaultAsync(vi => vi.Id == id);

        if (entity is null)
            return false;

        mapper.MapCreateOrUpdateVisitItemModelToVisitItem(model, entity);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteVisitItemAsync(Guid id)
    {
        var entity = await context.VisitItems
            .FirstOrDefaultAsync(vi => vi.Id == id);

        if (entity is null)
            return false;

        context.VisitItems.Remove(entity);
        await context.SaveChangesAsync();
        return true;
    }
}
