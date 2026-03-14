using FireInvent.Database;
using FireInvent.Database.Extensions;
using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Database.Models;
using FireInvent.Shared.Extensions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class StorageLocationService(AppDbContext context, StorageLocationMapper mapper, StorageLocationMinStockMapper minStockMapper) : IStorageLocationService
{
    public async Task<StorageLocationModel> CreateStorageLocationAsync(CreateOrUpdateStorageLocationModel model, CancellationToken cancellationToken = default)
    {
        var exists = await context.StorageLocations
            .AnyAsync(s => s.Name == model.Name, cancellationToken);

        if (exists)
            throw new ConflictException($"A StorageLocation with name '{model.Name}' already exists.");

        var location = mapper.MapCreateOrUpdateStorageLocationModelToStorageLocation(model);

        await context.StorageLocations.AddAsync(location, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return mapper.MapStorageLocationToStorageLocationModel(location);
    }

    public async Task<PagedResult<StorageLocationModel>> GetAllStorageLocationsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var query = context.StorageLocations
            .OrderBy(sl => sl.Name)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectStorageLocationsToStorageLocationModels(query);

        var result = await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);

        var locationIds = result.Items.Select(x => x.Id).ToList();
        var warningLocationIds = await GetLocationIdsWithStockWarningsAsync(locationIds, cancellationToken);

        var enrichedItems = result.Items
            .Select(x => x with { HasStockWarning = warningLocationIds.Contains(x.Id) })
            .ToList();

        return result with { Items = enrichedItems };
    }

    public async Task<StorageLocationModel?> GetStorageLocationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var location = await context.StorageLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (location is null)
            return null;

        var model = mapper.MapStorageLocationToStorageLocationModel(location);
        var warningIds = await GetLocationIdsWithStockWarningsAsync([id], cancellationToken);
        return model with { HasStockWarning = warningIds.Contains(id) };
    }

    public async Task<bool> UpdateStorageLocationAsync(Guid id, CreateOrUpdateStorageLocationModel model, CancellationToken cancellationToken = default)
    {
        var location = await context.StorageLocations.FindAsync(id, cancellationToken);
        if (location is null)
            return false;

        var nameExists = await context.StorageLocations
            .AnyAsync(s => s.Name == model.Name && s.Id != id, cancellationToken);

        if (nameExists)
            throw new ConflictException($"A StorageLocation with name '{model.Name}' already exists.");

        mapper.MapCreateOrUpdateStorageLocationModelToStorageLocation(model, location);

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteStorageLocationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.TryDeleteEntityAsync(
            id,
            nameof(StorageLocation),
            context.StorageLocations,
            cancellationToken);
    }

    public async Task<List<StorageLocationMinStockModel>> GetMinStocksForStorageLocationAsync(Guid storageLocationId, CancellationToken cancellationToken = default)
    {
        var locationExists = await context.StorageLocations
            .AnyAsync(s => s.Id == storageLocationId, cancellationToken);

        if (!locationExists)
            throw new NotFoundException($"StorageLocation with ID '{storageLocationId}' does not exist.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var minStocks = await context.StorageLocationMinStocks
            .Where(ms => ms.StorageLocationId == storageLocationId)
            .Select(ms => new
            {
                ms.Id,
                ms.StorageLocationId,
                ms.VariantId,
                VariantName = ms.Variant.Name,
                ProductName = ms.Variant.Product.Name,
                ms.MinStock,
                ms.CreatedAt,
                ms.ModifiedAt,
                CurrentStock = context.ItemAssignmentHistories.Count(a =>
                    a.StorageLocationId == storageLocationId &&
                    a.Item.VariantId == ms.VariantId &&
                    a.AssignedFrom <= today &&
                    (a.AssignedUntil == null || a.AssignedUntil >= today))
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return minStocks.Select(ms => new StorageLocationMinStockModel
        {
            Id = ms.Id,
            StorageLocationId = ms.StorageLocationId,
            VariantId = ms.VariantId,
            VariantName = ms.VariantName,
            ProductName = ms.ProductName,
            MinStock = ms.MinStock,
            CurrentStock = ms.CurrentStock,
            CreatedAt = ms.CreatedAt,
            ModifiedAt = ms.ModifiedAt
        }).ToList();
    }

    public async Task<StorageLocationMinStockModel> SetMinStockAsync(Guid storageLocationId, CreateOrUpdateStorageLocationMinStockModel model, CancellationToken cancellationToken = default)
    {
        var locationExists = await context.StorageLocations
            .AnyAsync(s => s.Id == storageLocationId, cancellationToken);

        if (!locationExists)
            throw new NotFoundException($"StorageLocation with ID '{storageLocationId}' does not exist.");

        var variantExists = await context.Variants
            .AnyAsync(v => v.Id == model.VariantId, cancellationToken);

        if (!variantExists)
            throw new BadRequestException($"Variant with ID '{model.VariantId}' does not exist.");

        var existing = await context.StorageLocationMinStocks
            .FirstOrDefaultAsync(ms => ms.StorageLocationId == storageLocationId && ms.VariantId == model.VariantId, cancellationToken);

        if (existing is null)
        {
            var newEntry = minStockMapper.MapCreateOrUpdateStorageLocationMinStockModelToStorageLocationMinStock(model);
            newEntry.StorageLocationId = storageLocationId;
            context.StorageLocationMinStocks.Add(newEntry);
            await context.SaveChangesAsync(cancellationToken);
            existing = newEntry;
        }
        else
        {
            minStockMapper.MapCreateOrUpdateStorageLocationMinStockModelToStorageLocationMinStock(model, existing);
            await context.SaveChangesAsync(cancellationToken);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currentStock = await context.ItemAssignmentHistories.CountAsync(a =>
            a.StorageLocationId == storageLocationId &&
            a.Item.VariantId == existing.VariantId &&
            a.AssignedFrom <= today &&
            (a.AssignedUntil == null || a.AssignedUntil >= today), cancellationToken);

        var variant = await context.Variants
            .AsNoTracking()
            .Select(v => new { v.Id, v.Name, ProductName = v.Product.Name })
            .FirstOrDefaultAsync(v => v.Id == existing.VariantId, cancellationToken);

        return new StorageLocationMinStockModel
        {
            Id = existing.Id,
            StorageLocationId = existing.StorageLocationId,
            VariantId = existing.VariantId,
            VariantName = variant?.Name ?? string.Empty,
            ProductName = variant?.ProductName ?? string.Empty,
            MinStock = existing.MinStock,
            CurrentStock = currentStock,
            CreatedAt = existing.CreatedAt,
            ModifiedAt = existing.ModifiedAt
        };
    }

    public async Task<bool> DeleteMinStockAsync(Guid storageLocationId, Guid variantId, CancellationToken cancellationToken = default)
    {
        var entry = await context.StorageLocationMinStocks
            .FirstOrDefaultAsync(ms => ms.StorageLocationId == storageLocationId && ms.VariantId == variantId, cancellationToken);

        if (entry is null)
            return false;

        context.StorageLocationMinStocks.Remove(entry);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    internal async Task<HashSet<Guid>> GetLocationIdsWithStockWarningsAsync(List<Guid> locationIds, CancellationToken cancellationToken)
    {
        if (locationIds.Count == 0)
            return [];

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var minStocks = await context.StorageLocationMinStocks
            .Where(ms => locationIds.Contains(ms.StorageLocationId))
            .Select(ms => new
            {
                ms.StorageLocationId,
                ms.VariantId,
                ms.MinStock,
                CurrentStock = context.ItemAssignmentHistories.Count(a =>
                    a.StorageLocationId == ms.StorageLocationId &&
                    a.Item.VariantId == ms.VariantId &&
                    a.AssignedFrom <= today &&
                    (a.AssignedUntil == null || a.AssignedUntil >= today))
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return minStocks
            .Where(ms => ms.CurrentStock < ms.MinStock)
            .Select(ms => ms.StorageLocationId)
            .ToHashSet();
    }
}
