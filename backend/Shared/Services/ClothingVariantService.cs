using AutoMapper;
using FireInvent.Database;
using FireInvent.Database.Models;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class ClothingVariantService(GearDbContext context, IMapper mapper)
{
    public async Task<ClothingVariantModel> CreateVariantAsync(CreateClothingVariantModel model)
    {
        var duplicate = await context.ClothingVariants.AnyAsync(v =>
            v.ProductId == model.ProductId &&
            v.Name == model.Name);

        if (duplicate)
            throw new ConflictException("A ClothingVariant with this name already exists for this product.");

        var productExists = await context.ClothingProducts.AnyAsync(p => p.Id == model.ProductId);
        if (!productExists)
            throw new BadRequestException("Referenced product does not exist.");

        var variant = mapper.Map<ClothingVariant>(model);
        variant.Id = Guid.NewGuid();

        context.ClothingVariants.Add(variant);
        await context.SaveChangesAsync();

        return mapper.Map<ClothingVariantModel>(variant);
    }

    public async Task<List<ClothingVariantModel>> GetAllVariantsAsync()
    {
        var variants = await context.ClothingVariants
            .AsNoTracking()
            .OrderBy(v => v.Name)
            .ToListAsync();

        return mapper.Map<List<ClothingVariantModel>>(variants);
    }

    public async Task<ClothingVariantModel?> GetVariantByIdAsync(Guid id)
    {
        var variant = await context.ClothingVariants
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);

        return variant is null ? null : mapper.Map<ClothingVariantModel>(variant);
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

        mapper.Map(model, variant);

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

    public async Task<List<ClothingVariantModel>> GetVariantsForProductAsync(Guid productId)
    {
        var productExists = await context.ClothingProducts.AnyAsync(v => v.Id == productId);
        if (!productExists)
            throw new NotFoundException($"ClothingProduct with ID {productId} not found.");

        var items = await context.ClothingVariants
            .Where(i => i.ProductId == productId)
            .OrderBy(v => v.Name)
            .AsNoTracking()
            .ToListAsync();

        return mapper.Map<List<ClothingVariantModel>>(items);
    }
}
