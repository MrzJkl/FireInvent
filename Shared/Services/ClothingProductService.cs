using FlameGuardLaundry.Database;
using FlameGuardLaundry.Database.Models;
using FlameGuardLaundry.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlameGuardLaundry.Shared.Services
{
    public class ClothingProductService(GearDbContext context)
    {
        public async Task<ClothingProductModel> CreateProductAsync(ClothingProductModel model)
        {
            var exists = await context.ClothingProducts.AnyAsync(p =>
                p.Name == model.Name && p.Manufacturer == model.Manufacturer);

            if (exists)
                throw new InvalidOperationException("A product with the same name and manufacturer already exists.");

            var product = new ClothingProduct
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Manufacturer = model.Manufacturer,
                Description = model.Description,
                Type = model.Type
            };

            context.ClothingProducts.Add(product);
            await context.SaveChangesAsync();

            return model with { Id = product.Id };
        }

        public async Task<List<ClothingProductModel>> GetAllProductsAsync()
        {
            return await context.ClothingProducts
                .AsNoTracking()
                .Select(p => new ClothingProductModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Manufacturer = p.Manufacturer,
                    Description = p.Description,
                    Type = p.Type
                })
                .ToListAsync();
        }

        public async Task<ClothingProductModel?> GetProductByIdAsync(Guid id)
        {
            var product = await context.ClothingProducts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
                return null;

            return new ClothingProductModel
            {
                Id = product.Id,
                Name = product.Name,
                Manufacturer = product.Manufacturer,
                Description = product.Description,
                Type = product.Type
            };
        }

        public async Task<bool> UpdateProductAsync(ClothingProductModel model)
        {
            var product = await context.ClothingProducts.FindAsync(model.Id);
            if (product is null)
                return false;

            var duplicate = await context.ClothingProducts.AnyAsync(p =>
                p.Id != model.Id &&
                p.Name == model.Name &&
                p.Manufacturer == model.Manufacturer);

            if (duplicate)
                throw new InvalidOperationException("Another product with the same name and manufacturer already exists.");

            product.Name = model.Name;
            product.Manufacturer = model.Manufacturer;
            product.Description = model.Description;
            product.Type = model.Type;

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
}
