using AutoMapper;
using FireInvent.Database;
using FireInvent.Database.Models;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class ClothingProductService(AppDbContext context, IMapper mapper) : IClothingProductService
{
    public async Task<ProductModel> CreateProductAsync(CreateProductModel model)
    {
        var exists = await context.ClothingProducts.AnyAsync(p =>
            p.Name == model.Name && p.Manufacturer == model.Manufacturer);

        if (exists)
            throw new InvalidOperationException("A product with the same name and manufacturer already exists.");

        var product = mapper.Map<Product>(model);
        product.Id = Guid.NewGuid();

        context.ClothingProducts.Add(product);
        await context.SaveChangesAsync();

        return mapper.Map<ProductModel>(product);
    }

    public async Task<List<ProductModel>> GetAllProductsAsync()
    {
        var products = await context.ClothingProducts
            .OrderBy(p => p.Name)
            .AsNoTracking()
            .ToListAsync();

        return mapper.Map<List<ProductModel>>(products);
    }

    public async Task<ProductModel?> GetProductByIdAsync(Guid id)
    {
        var product = await context.ClothingProducts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        return product is null ? null : mapper.Map<ProductModel>(product);
    }

    public async Task<bool> UpdateProductAsync(ProductModel model)
    {
        var product = await context.ClothingProducts.FindAsync(model.Id);
        if (product is null)
            return false;

        var duplicate = await context.ClothingProducts.AnyAsync(p =>
            p.Id != model.Id &&
            p.Name == model.Name &&
            p.Manufacturer == model.Manufacturer);

        if (duplicate)
            throw new ConflictException();

        mapper.Map(model, product);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        var product = await context.ClothingProducts.FindAsync(id);
        if (product is null)
            return false;

        context.ClothingProducts.Remove(product);
        await context.SaveChangesAsync();
        return true;
    }
}