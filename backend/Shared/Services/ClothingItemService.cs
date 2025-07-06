using AutoMapper;
using FlameGuardLaundry.Database;
using FlameGuardLaundry.Database.Models;
using FlameGuardLaundry.Shared.Exceptions;
using FlameGuardLaundry.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FlameGuardLaundry.Shared.Services;

public class ClothingItemService(GearDbContext context, IMapper mapper)
{
    public async Task<ClothingItemModel> CreateClothingItemAsync(CreateClothingItemModel model)
    {
        if (!await context.ClothingVariants.AnyAsync(v => v.Id == model.VariantId))
            throw new BadRequestException("ClothingVariant not found.");

        if (model.StorageLocationId.HasValue &&
            !await context.StorageLocations.AnyAsync(s => s.Id == model.StorageLocationId))
            throw new BadRequestException("StorageLocation not found.");

        if (!string.IsNullOrWhiteSpace(model.Identifier))
        {
            var exists = await context.ClothingItems
                .AnyAsync(c => c.Identifier == model.Identifier);

            if (exists)
                throw new ConflictException($"ClothingItem with identifier '{model.Identifier}' already exists.");
        }

        var item = mapper.Map<ClothingItem>(model);
        item.Id = Guid.NewGuid();

        context.ClothingItems.Add(item);
        await context.SaveChangesAsync();

        return mapper.Map<ClothingItemModel>(item);
    }

    public async Task<List<ClothingItemModel>> GetAllClothingItemsAsync()
    {
        var items = await context.ClothingItems
            .AsNoTracking()
            .OrderBy(i => i.PurchaseDate)
            .ToListAsync();

        return mapper.Map<List<ClothingItemModel>>(items);
    }

    public async Task<ClothingItemModel?> GetClothingItemByIdAsync(Guid id)
    {
        var item = await context.ClothingItems
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);

        return item is null ? null : mapper.Map<ClothingItemModel>(item);
    }

    public async Task<bool> UpdateClothingItemAsync(ClothingItemModel model)
    {
        var item = await context.ClothingItems.FindAsync(model.Id);
        if (item is null)
            return false;

        if (!await context.ClothingVariants.AnyAsync(v => v.Id == model.VariantId))
            throw new BadRequestException("ClothingVariant not found.");

        if (model.StorageLocationId.HasValue &&
            !await context.StorageLocations.AnyAsync(s => s.Id == model.StorageLocationId))
            throw new BadRequestException("StorageLocation not found.");

        if (!string.IsNullOrWhiteSpace(model.Identifier))
        {
            var exists = await context.ClothingItems
                .AnyAsync(c => c.Identifier == model.Identifier && c.Id != model.Id);

            if (exists)
                throw new ConflictException($"ClothingItem with identifier '{model.Identifier}' already exists.");
        }

        mapper.Map(model, item);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteClothingItemAsync(Guid id)
    {
        var item = await context.ClothingItems.FindAsync(id);
        if (item is null)
            return false;

        context.ClothingItems.Remove(item);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ClothingItemModel>> GetItemsForVariantAsync(Guid variantId)
    {
        var variantExists = await context.ClothingVariants.AnyAsync(v => v.Id == variantId);
        if (!variantExists)
            throw new NotFoundException($"ClothingVariant with ID {variantId} not found.");

        var items = await context.ClothingItems
            .Where(i => i.VariantId == variantId)
            .OrderBy(i => i.PurchaseDate)
            .AsNoTracking()
            .ToListAsync();

        return mapper.Map<List<ClothingItemModel>>(items);
    }

    public async Task<List<ClothingItemModel>> GetClothingItemsForStorageLocationAsync(Guid storageLocationId)
    {
        var locationExists = await context.StorageLocations.AnyAsync(s => s.Id == storageLocationId);
        if (!locationExists)
            throw new NotFoundException($"StorageLocation with ID {storageLocationId} not found.");

        var items = await context.ClothingItems
            .Where(i => i.StorageLocationId == storageLocationId)
            .OrderBy(i => i.PurchaseDate)
            .AsNoTracking()
            .ToListAsync();

        return mapper.Map<List<ClothingItemModel>>(items);
    }
}