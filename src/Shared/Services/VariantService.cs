using FireInvent.Database;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class VariantService(AppDbContext context, VariantMapper mapper) : IVariantService
{
    public async Task<VariantModel> CreateVariantAsync(CreateVariantModel model)
    {
        var duplicate = await context.Variants.AnyAsync(v =>
            v.ProductId == model.ProductId &&
            v.Name == model.Name);

        if (duplicate)
            throw new ConflictException("A Variant with this name already exists for this product.");

        var productExists = await context.Products.AnyAsync(p => p.Id == model.ProductId);
        if (!productExists)
            throw new BadRequestException("Referenced product does not exist.");

        var variant = mapper.MapCreateVariantModelToVariant(model);
        variant.Id = Guid.NewGuid();

        context.Variants.Add(variant);
        await context.SaveChangesAsync();

        return mapper.MapVariantToVariantModel(variant);
    }

    public async Task<List<VariantModel>> GetAllVariantsAsync()
    {
        var variants = await context.Variants
            .AsNoTracking()
            .OrderBy(v => v.Name)
            .ToListAsync();

        return mapper.MapVariantsToVariantModels(variants);
    }

    public async Task<VariantModel?> GetVariantByIdAsync(Guid id)
    {
        var variant = await context.Variants
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);

        return variant is null ? null : mapper.MapVariantToVariantModel(variant);
    }

    public async Task<bool> UpdateVariantAsync(VariantModel model)
    {
        var variant = await context.Variants.FindAsync(model.Id);
        if (variant is null)
            return false;

        var duplicate = await context.Variants.AnyAsync(v =>
            v.Id != model.Id &&
            v.ProductId == model.ProductId &&
            v.Name == model.Name);

        if (duplicate)
            throw new ConflictException("Another variant with the same name already exists for this product.");

        mapper.MapVariantModelToVariant(model, variant);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteVariantAsync(Guid id)
    {
        var variant = await context.Variants.FindAsync(id);
        if (variant is null)
            return false;

        context.Variants.Remove(variant);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<List<VariantModel>> GetVariantsForProductAsync(Guid productId)
    {
        var productExists = await context.Products.AnyAsync(v => v.Id == productId);
        if (!productExists)
            throw new NotFoundException($"ClothingProduct with ID {productId} not found.");

        var items = await context.Variants
            .Where(i => i.ProductId == productId)
            .OrderBy(v => v.Name)
            .AsNoTracking()
            .ToListAsync();

        return mapper.MapVariantsToVariantModels(items);
    }
}
