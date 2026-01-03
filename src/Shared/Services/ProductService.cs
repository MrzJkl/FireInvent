using FireInvent.Database;
using FireInvent.Database.Extensions;
using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Extensions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class ProductService(AppDbContext context, ProductMapper mapper) : IProductService
{
    public async Task<ProductModel> CreateProductAsync(CreateOrUpdateProductModel model)
    {
        _ = await context.ProductTypes.FindAsync(model.TypeId) ?? throw new BadRequestException($"ProductType with ID '{model.TypeId}' does not exist.");
        _ = await context.Manufacturers.FindAsync(model.ManufacturerId) ?? throw new BadRequestException($"Manufacturer with ID '{model.ManufacturerId}' does not exist.");

        var nameExists = await context.Products.AnyAsync(p =>
            p.Name == model.Name && p.ManufacturerId == model.ManufacturerId);

        if (nameExists)
            throw new ConflictException("A product with the same name and manufacturer already exists.");

        if (!string.IsNullOrEmpty(model.ExternalIdentifier))
        {
            var externalIdDuplicate = await context.Products.AnyAsync(p =>
                p.ExternalIdentifier == model.ExternalIdentifier && p.ManufacturerId == model.ManufacturerId);
            if (externalIdDuplicate)
                throw new ConflictException("A product with the same external identifier and manufacturer already exists.");
        }

        var product = mapper.MapCreateOrUpdateProductModelToProduct(model);

        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        product = await context.Products
            .AsNoTracking()
            .SingleAsync(p => p.Id == product.Id);

        return mapper.MapProductToProductModel(product);
    }

    public async Task<PagedResult<ProductModel>> GetAllProductsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var query = context.Products
            .OrderBy(p => p.Name)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectProductsToProductModels(query);

        return await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);
    }

    public async Task<ProductModel?> GetProductByIdAsync(Guid id)
    {
        var product = await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        return product is null ? null : mapper.MapProductToProductModel(product);
    }

    public async Task<bool> UpdateProductAsync(Guid id, CreateOrUpdateProductModel model)
    {
        _ = await context.ProductTypes.FindAsync(model.TypeId) ?? throw new BadRequestException($"ProductType with ID '{model.TypeId}' does not exist.");
        _ = await context.Manufacturers.FindAsync(model.ManufacturerId) ?? throw new BadRequestException($"Manufacturer with ID '{model.ManufacturerId}' does not exist.");

        var product = await context.Products.FindAsync(id);
        if (product is null)
            return false;

        var nameDuplicate = await context.Products.AnyAsync(p =>
            p.Id != id &&
            p.Name == model.Name &&
            p.ManufacturerId == model.ManufacturerId);

        if (nameDuplicate)
            throw new ConflictException("A product with the same name and manufacturer already exists.");

        if (!string.IsNullOrEmpty(model.ExternalIdentifier))
        {
            var externalIdDuplicate = await context.Products.AnyAsync(p =>
                p.ExternalIdentifier == model.ExternalIdentifier && p.ManufacturerId == model.ManufacturerId && p.Id != id);
            if (externalIdDuplicate)
                throw new ConflictException("A product with the same external identifier and manufacturer already exists.");
        }

        mapper.MapCreateOrUpdateProductModelToProduct(model, product);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var product = await context.Products.FindAsync(id);
        if (product is null)
            return false;

        context.Products.Remove(product);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<ProductModel>> GetProductsForManufacturer(Guid manufacturerId, PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var manufacturerExists = await context.Manufacturers.AnyAsync(v => v.Id == manufacturerId);
        if (!manufacturerExists)
            throw new NotFoundException($"Manufacturer with ID {manufacturerId} not found.");

        var query = context.Products
            .Where(p => p.ManufacturerId == manufacturerId)
            .OrderBy(p => p.Name)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectProductsToProductModels(query);

        return await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);
    }
}