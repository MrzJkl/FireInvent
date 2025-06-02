using FlameGuardLaundry.Database;
using FlameGuardLaundry.Database.Models;
using FlameGuardLaundry.Shared.Exceptions;
using FlameGuardLaundry.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FlameGuardLaundry.Shared.Services
{
    public class ClothingItemService(GearDbContext context)
    {
        public async Task<ClothingItemModel> CreateClothingItemAsync(ClothingItemModel model)
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

            var item = new ClothingItem
            {
                Id = Guid.NewGuid(),
                VariantId = model.VariantId,
                Identifier = model.Identifier,
                StorageLocationId = model.StorageLocationId,
                Condition = model.Condition,
                PurchaseDate = model.PurchaseDate,
                RetirementDate = model.RetirementDate
            };

            context.ClothingItems.Add(item);
            await context.SaveChangesAsync();

            return new ClothingItemModel
            {
                Id = item.Id,
                VariantId = item.VariantId,
                Identifier = item.Identifier,
                StorageLocationId = item.StorageLocationId,
                Condition = item.Condition,
                PurchaseDate = item.PurchaseDate,
                RetirementDate = item.RetirementDate
            };
        }

        public async Task<List<ClothingItemModel>> GetAllClothingItemsAsync()
        {
            return await context.ClothingItems
                .AsNoTracking()
                .Select(i => new ClothingItemModel
                {
                    Id = i.Id,
                    VariantId = i.VariantId,
                    Identifier = i.Identifier,
                    StorageLocationId = i.StorageLocationId,
                    Condition = i.Condition,
                    PurchaseDate = i.PurchaseDate,
                    RetirementDate = i.RetirementDate
                })
                .ToListAsync();
        }

        public async Task<ClothingItemModel?> GetClothingItemByIdAsync(Guid id)
        {
            var item = await context.ClothingItems
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);

            return item is null ? null : new ClothingItemModel
            {
                Id = item.Id,
                VariantId = item.VariantId,
                Identifier = item.Identifier,
                StorageLocationId = item.StorageLocationId,
                Condition = item.Condition,
                PurchaseDate = item.PurchaseDate,
                RetirementDate = item.RetirementDate
            };
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

            item.VariantId = model.VariantId;
            item.Identifier = model.Identifier;
            item.StorageLocationId = model.StorageLocationId;
            item.Condition = model.Condition;
            item.PurchaseDate = model.PurchaseDate;
            item.RetirementDate = model.RetirementDate;

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
    }

}
