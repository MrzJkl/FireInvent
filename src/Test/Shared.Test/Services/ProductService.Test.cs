using FireInvent.Database;
using FireInvent.Database.Models;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Test.Shared.Services;

public class ProductServiceTest
{
    private readonly ProductMapper _mapper;

    public ProductServiceTest()
    {
        _mapper = new ProductMapper();
    }

    private async Task<ProductType> AddProductTypeAsync(AppDbContext context)
    {
        var type = new ProductType
        {
            Id = Guid.NewGuid(),
            Name = "Jacket"
        };
        context.ProductTypes.Add(type);
        await context.SaveChangesAsync();
        return type;
    }

    [Fact]
    public async Task CreateProductAsync_ShouldCreateProduct()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new ProductService(context, _mapper);
        var productType = await AddProductTypeAsync(context);

        var model = new CreateOrUpdateProductModel
        {
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            TypeId = productType.Id,
        };

        var result = await service.CreateProductAsync(model);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.Name, result.Name);
        Assert.Equal(model.Manufacturer, result.Manufacturer);
        Assert.Equal(model.Description, result.Description);
        Assert.Equal(model.TypeId, result.TypeId);

        var entity = await context.Products.FindAsync(result.Id);
        Assert.NotNull(entity);
        Assert.Equal(model.Name, entity!.Name);
        Assert.Equal(model.Manufacturer, entity.Manufacturer);
        Assert.Equal(model.Description, entity.Description);
        Assert.Equal(model.TypeId, entity.TypeId);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldThrowIfDuplicate()
    {
        var context = TestHelper.GetTestDbContext();
        var productType = await AddProductTypeAsync(context);

        context.Products.Add(new Product
        {
            Id = Guid.NewGuid(),
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            TypeId = productType.Id,
        });
        context.SaveChanges();

        var service = new ProductService(context, _mapper);

        var model = new CreateOrUpdateProductModel
        {
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            TypeId = productType.Id,
        };

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateProductAsync(model));
    }

    [Fact]
    public async Task GetAllProductsAsync_ShouldReturnAllProducts()
    {
        var context = TestHelper.GetTestDbContext();
        var productType = await AddProductTypeAsync(context);

        var product1 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            TypeId = productType.Id,
        };
        var product2 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Pants",
            Manufacturer = "BrandB",
            Description = "Fire-resistant pants",
            TypeId = productType.Id,
        };
        context.Products.Add(product1);
        context.Products.Add(product2);
        context.SaveChanges();

        var service = new ProductService(context, _mapper);

        var result = await service.GetAllProductsAsync();

        Assert.Equal(2, result.Count);
        var jacket = result.FirstOrDefault(p => p.Name == "Jacket");
        var pants = result.FirstOrDefault(p => p.Name == "Pants");

        Assert.NotNull(jacket);
        Assert.Equal(product1.Id, jacket!.Id);
        Assert.Equal(product1.Manufacturer, jacket.Manufacturer);
        Assert.Equal(product1.Description, jacket.Description);
        Assert.Equal(product1.TypeId, jacket.TypeId);

        Assert.NotNull(pants);
        Assert.Equal(product2.Id, pants!.Id);
        Assert.Equal(product2.Manufacturer, pants.Manufacturer);
        Assert.Equal(product2.Description, pants.Description);
        Assert.Equal(product2.TypeId, pants.TypeId);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnProduct()
    {
        var context = TestHelper.GetTestDbContext();
        var productType = await AddProductTypeAsync(context);

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            TypeId = productType.Id,
        };
        context.Products.Add(product);
        context.SaveChanges();

        var service = new ProductService(context, _mapper);

        var result = await service.GetProductByIdAsync(product.Id);

        Assert.NotNull(result);
        Assert.Equal(product.Id, result!.Id);
        Assert.Equal(product.Name, result.Name);
        Assert.Equal(product.Manufacturer, result.Manufacturer);
        Assert.Equal(product.Description, result.Description);
        Assert.Equal(product.TypeId, result.TypeId);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnNullIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new ProductService(context, _mapper);

        var result = await service.GetProductByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldUpdateProduct()
    {
        var context = TestHelper.GetTestDbContext();
        var productType = await AddProductTypeAsync(context);

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            TypeId = productType.Id,
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var allproducts = await context.Products.ToListAsync();

        var service = new ProductService(context, _mapper);

        var model = new ProductModel
        {
            Id = product.Id,
            Name = "Jacket Updated",
            Manufacturer = "BrandA Updated",
            Description = "Updated description",
            TypeId = productType.Id,
        };

        var result = await service.UpdateProductAsync(model);

        Assert.True(result);
        var updated = await context.Products.FindAsync(product.Id);
        var allproducts2 = await context.Products.ToListAsync();
        Assert.NotNull(updated);
        Assert.Equal(model.Name, updated!.Name);
        Assert.Equal(model.Manufacturer, updated.Manufacturer);
        Assert.Equal(model.Description, updated.Description);
        Assert.Equal(model.TypeId, updated.TypeId);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldReturnFalseIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new ProductService(context, _mapper);
        var productType = await AddProductTypeAsync(context);

        var model = new ProductModel
        {
            Id = Guid.NewGuid(),
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            TypeId = productType.Id,
        };

        var result = await service.UpdateProductAsync(model);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldThrowIfDuplicate()
    {
        var context = TestHelper.GetTestDbContext();
        var productType = await AddProductTypeAsync(context);

        var product1 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            TypeId = productType.Id,
        };
        var product2 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Pants",
            Manufacturer = "BrandB",
            Description = "Fire-resistant pants",
            TypeId = productType.Id,
        };
        context.Products.Add(product1);
        context.Products.Add(product2);
        context.SaveChanges();

        var service = new ProductService(context, _mapper);

        var model = new ProductModel
        {
            Id = product2.Id,
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Fire-resistant pants",
            TypeId = productType.Id,
        };

        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateProductAsync(model));
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldDeleteProduct()
    {
        var context = TestHelper.GetTestDbContext();
        var productType = await AddProductTypeAsync(context);

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            TypeId = productType.Id,
        };
        context.Products.Add(product);
        context.SaveChanges();

        var service = new ProductService(context, _mapper);

        var result = await service.DeleteProductAsync(product.Id);

        Assert.True(result);
        Assert.False(context.Products.Any());
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldReturnFalseIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new ProductService(context, _mapper);

        var result = await service.DeleteProductAsync(Guid.NewGuid());

        Assert.False(result);
    }
}