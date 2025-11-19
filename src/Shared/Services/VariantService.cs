using FireInvent.Database;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class VariantService(AppDbContext context, VariantMapper mapper) : IVariantService
{
    public async Task<VariantModel> CreateVariantAsync(CreateOrUpdateVariantModel model)
    {
        var duplicate = await context.Variants.AnyAsync(v =>
            v.ProductId == model.ProductId &&
            v.Name == model.Name);

        if (duplicate)
            throw new ConflictException("A Variant with this name already exists for this product.");

        var productExists = await context.Products.AnyAsync(p => p.Id == model.ProductId);
        if (!productExists)
            throw new BadRequestException("Referenced product does not exist.");

        var variant = mapper.MapCreateOrUpdateVariantModelToVariant(model);
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

    public async Task<bool> UpdateVariantAsync(Guid id, CreateOrUpdateVariantModel model)
    {
        var variant = await context.Variants.FindAsync(id);
        if (variant is null)
            return false;

        _ = await context.Products.FindAsync(model.ProductId) ?? throw new BadRequestException($"Product with ID '{model.ProductId}' does not exist.");

        var duplicate = await context.Variants.AnyAsync(v =>
            v.Id != id &&
            v.ProductId == model.ProductId &&
            v.Name == model.Name);

        if (duplicate)
            throw new ConflictException("Another variant with the same name already exists for this product.");

        mapper.MapCreateOrUpdateVariantModelToVariant(model, variant, id);

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
            throw new NotFoundException($"Product with ID {productId} not found.");

        var items = await context.Variants
            .Where(i => i.ProductId == productId)
            .OrderBy(v => v.Name)
            .AsNoTracking()
            .ToListAsync();

        return mapper.MapVariantsToVariantModels(items);
    }
}
