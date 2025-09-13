using FireInvent.Database;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class ProductService(AppDbContext context, ProductMapper mapper) : IProductService
{
    public async Task<ProductModel> CreateProductAsync(CreateProductModel model)
    {
        var exists = await context.Products.AnyAsync(p =>
            p.Name == model.Name && p.Manufacturer == model.Manufacturer);

        if (exists)
            throw new ConflictException("A product with the same name and manufacturer already exists.");

        var product = mapper.MapCreateProductModelToProduct(model);

        context.Products.Add(product);
        await context.SaveChangesAsync();

        return mapper.MapProductToProductModel(product);
    }

    public async Task<List<ProductModel>> GetAllProductsAsync()
    {
        var products = await context.Products
            .OrderBy(p => p.Name)
            .AsNoTracking()
            .ToListAsync();

        return mapper.MapProductsToProductModels(products);
    }

    public async Task<ProductModel?> GetProductByIdAsync(Guid id)
    {
        var product = await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        return product is null ? null : mapper.MapProductToProductModel(product);
    }

    public async Task<bool> UpdateProductAsync(ProductModel model)
    {
        var product = await context.Products.FindAsync(model.Id);
        if (product is null)
            return false;

        var duplicate = await context.Products.AnyAsync(p =>
            p.Id != model.Id &&
            p.Name == model.Name &&
            p.Manufacturer == model.Manufacturer);

        if (duplicate)
            throw new ConflictException("A product with the same name and manufacturer already exists.");

        mapper.MapProductModelToProduct(model, product);

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
}