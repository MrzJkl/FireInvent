using FireInvent.Database;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class VariantService(AppDbContext context, VariantMapper mapper) : IVariantService
{
    public async Task<VariantModel> CreateVariantAsync(CreateOrUpdateVariantModel model)
    {
        _ = await context.Products.FindAsync(model.ProductId) ?? throw new BadRequestException($"Product with ID '{model.ProductId}' does not exist.");
        
        var nameDuplicate = await context.Variants.AnyAsync(v =>
            v.ProductId == model.ProductId &&
            v.Name == model.Name);

        if (nameDuplicate)
            throw new ConflictException("A Variant with this name already exists for this product.");

        if (!string.IsNullOrEmpty(model.ExternalIdentifier))
        {
            var externalIdDuplicate = await context.Variants.AnyAsync(v =>
                v.ExternalIdentifier == model.ExternalIdentifier && v.ProductId == model.ProductId);
            if (externalIdDuplicate)
                throw new ConflictException("A variant with the same external identifier and product already exists.");
        }

        var variant = mapper.MapCreateOrUpdateVariantModelToVariant(model);

        await context.Variants.AddAsync(variant);
        await context.SaveChangesAsync();

        variant = await context.Variants
            .AsNoTracking()
            .SingleAsync(v => v.Id == variant.Id);

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

        var nameDuplicate = await context.Variants.AnyAsync(v =>
            v.Id != id &&
            v.ProductId == model.ProductId &&
            v.Name == model.Name);

        if (nameDuplicate)
            throw new ConflictException("Another variant with the same name already exists for this product.");

        if (!string.IsNullOrEmpty(model.ExternalIdentifier))
        {
            var externalIdDuplicate = await context.Variants.AnyAsync(v =>
                v.ExternalIdentifier == model.ExternalIdentifier && v.ProductId == model.ProductId && v.Id != id);
            
            if (externalIdDuplicate)
                throw new ConflictException("A variant with the same external identifier and product already exists.");
        }

        mapper.MapCreateOrUpdateVariantModelToVariant(model, variant);

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
