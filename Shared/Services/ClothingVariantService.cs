using FlameGuardLaundry.Database;
using FlameGuardLaundry.Database.Models;
using FlameGuardLaundry.Shared.Exceptions;
using FlameGuardLaundry.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FlameGuardLaundry.Shared.Services
{
    public class ClothingVariantService(GearDbContext context)
    {
        public async Task<ClothingVariantModel> CreateVariantAsync(ClothingVariantModel model)
        {
            var duplicate = await context.ClothingVariants.AnyAsync(v =>
                v.ProductId == model.ProductId &&
                v.Name == model.Name);

            if (duplicate)
                throw new ConflictException("A ClothingVariant with this name already exists for this product.");

            var productExists = await context.ClothingProducts.AnyAsync(p => p.Id == model.ProductId);
            if (!productExists)
                throw new BadRequestException("Referenced product does not exist.");

            var variant = new ClothingVariant
            {
                Id = Guid.NewGuid(),
                ProductId = model.ProductId,
                Name = model.Name,
                AdditionalSpecs = model.AdditionalSpecs
            };

            context.ClothingVariants.Add(variant);
            await context.SaveChangesAsync();

            return model with { Id = variant.Id };
        }

        public async Task<List<ClothingVariantModel>> GetAllVariantsAsync()
        {
            return await context.ClothingVariants
                .AsNoTracking()
                .Select(v => new ClothingVariantModel
                {
                    Id = v.Id,
                    ProductId = v.ProductId,
                    Name = v.Name,
                    AdditionalSpecs = v.AdditionalSpecs
                })
                .ToListAsync();
        }

        public async Task<ClothingVariantModel?> GetVariantByIdAsync(Guid id)
        {
            var variant = await context.ClothingVariants
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);

            if (variant is null) return null;

            return new ClothingVariantModel
            {
                Id = variant.Id,
                ProductId = variant.ProductId,
                Name = variant.Name,
                AdditionalSpecs = variant.AdditionalSpecs
            };
        }

        public async Task<bool> UpdateVariantAsync(ClothingVariantModel model)
        {
            var variant = await context.ClothingVariants.FindAsync(model.Id);
            if (variant is null)
                return false;

            var duplicate = await context.ClothingVariants.AnyAsync(v =>
                v.Id != model.Id &&
                v.ProductId == model.ProductId &&
                v.Name == model.Name);

            if (duplicate)
                throw new ConflictException("Another variant with the same name already exists for this product.");

            variant.Name = model.Name;
            variant.AdditionalSpecs = model.AdditionalSpecs;
            variant.ProductId = model.ProductId;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteVariantAsync(Guid id)
        {
            var variant = await context.ClothingVariants.FindAsync(id);
            if (variant is null)
                return false;

            context.ClothingVariants.Remove(variant);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ClothingItemModel>> GetItemsForVariantAsync(Guid variantId)
        {
            var variantExists = await context.ClothingVariants.AnyAsync(v => v.Id == variantId);
            if (!variantExists)
                throw new NotFoundException($"ClothingVariant with ID {variantId} not found.");

            return await context.ClothingItems
                .Where(i => i.VariantId == variantId)
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
    }
}
