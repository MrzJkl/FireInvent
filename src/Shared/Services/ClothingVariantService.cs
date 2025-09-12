using AutoMapper;
using FireInvent.Database;
using FireInvent.Database.Models;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class ClothingVariantService(AppDbContext context, IMapper mapper) : IClothingVariantService
{
    public async Task<VariantModel> CreateVariantAsync(CreateVariantModel model)
    {
        var duplicate = await context.ClothingVariants.AnyAsync(v =>
            v.ProductId == model.ProductId &&
            v.Name == model.Name);

        if (duplicate)
            throw new ConflictException("A ClothingVariant with this name already exists for this product.");

        var productExists = await context.ClothingProducts.AnyAsync(p => p.Id == model.ProductId);
        if (!productExists)
            throw new BadRequestException("Referenced product does not exist.");

        var variant = mapper.Map<Variant>(model);
        variant.Id = Guid.NewGuid();

        context.ClothingVariants.Add(variant);
        await context.SaveChangesAsync();

        return mapper.Map<VariantModel>(variant);
    }

    public async Task<List<VariantModel>> GetAllVariantsAsync()
    {
        var variants = await context.ClothingVariants
            .AsNoTracking()
            .OrderBy(v => v.Name)
            .ToListAsync();

        return mapper.Map<List<VariantModel>>(variants);
    }

    public async Task<VariantModel?> GetVariantByIdAsync(Guid id)
    {
        var variant = await context.ClothingVariants
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);

        return variant is null ? null : mapper.Map<VariantModel>(variant);
    }

    public async Task<bool> UpdateVariantAsync(VariantModel model)
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

    public async Task<List<VariantModel>> GetVariantsForProductAsync(Guid productId)
    {
        var productExists = await context.ClothingProducts.AnyAsync(v => v.Id == productId);
        if (!productExists)
            throw new NotFoundException($"ClothingProduct with ID {productId} not found.");

        var items = await context.ClothingVariants
            .Where(i => i.ProductId == productId)
            .OrderBy(v => v.Name)
            .AsNoTracking()
            .ToListAsync();

        return mapper.Map<List<VariantModel>>(items);
    }
}
