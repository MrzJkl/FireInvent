using AutoMapper;
using FireInvent.Database;
using FireInvent.Database.Models;
using FireInvent.Shared;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace FireInvent.Test.Shared.Services;

public class ClothingProductServiceTest
{
    private readonly IMapper _mapper;

    public ClothingProductServiceTest()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), new NullLoggerFactory());
        _mapper = config.CreateMapper();
    }

    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldCreateProduct()
    {
        var context = GetDbContext();
        var service = new ClothingProductService(context, _mapper);

        var model = new CreateClothingProductModel
        {
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Description",
            Type = Contract.GearType.Jacket,
        };

        var result = await service.CreateProductAsync(model);

        Assert.NotNull(result);
        Assert.Equal("Jacket", result.Name);
        Assert.Equal("BrandA", result.Manufacturer);
        Assert.True(context.ClothingProducts.Any());
    }

    [Fact]
    public async Task CreateProductAsync_ShouldThrowIfDuplicate()
    {
        var context = GetDbContext();
        context.ClothingProducts.Add(new ClothingProduct { Id = Guid.NewGuid(), Name = "Jacket", Manufacturer = "BrandA" });
        context.SaveChanges();

        var service = new ClothingProductService(context, _mapper);

        var model = new CreateClothingProductModel
        {
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Description",
            Type = Contract.GearType.Jacket,
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateProductAsync(model));
    }

    [Fact]
    public async Task GetAllProductsAsync_ShouldReturnAllProducts()
    {
        var context = GetDbContext();
        context.ClothingProducts.Add(new ClothingProduct { Id = Guid.NewGuid(), Name = "Jacket", Manufacturer = "BrandA", Description = "Description", Type = Contract.GearType.Jacket });
        context.ClothingProducts.Add(new ClothingProduct { Id = Guid.NewGuid(), Name = "Pants", Manufacturer = "BrandB", Description = "Description", Type = Contract.GearType.Pants });
        context.SaveChanges();

        var service = new ClothingProductService(context, _mapper);

        var result = await service.GetAllProductsAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Name == "Jacket");
        Assert.Contains(result, p => p.Name == "Pants");
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnProduct()
    {
        var context = GetDbContext();
        var product = new ClothingProduct { Id = Guid.NewGuid(), Name = "Jacket", Manufacturer = "BrandA" };
        context.ClothingProducts.Add(product);
        context.SaveChanges();

        var service = new ClothingProductService(context, _mapper);

        var result = await service.GetProductByIdAsync(product.Id);

        Assert.NotNull(result);
        Assert.Equal(product.Name, result!.Name);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnNullIfNotFound()
    {
        var context = GetDbContext();
        var service = new ClothingProductService(context, _mapper);

        var result = await service.GetProductByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldUpdateProduct()
    {
        var context = GetDbContext();
        var product = new ClothingProduct { Id = Guid.NewGuid(), Name = "Jacket", Manufacturer = "BrandA" };
        context.ClothingProducts.Add(product);
        context.SaveChanges();

        var service = new ClothingProductService(context, _mapper);

        var model = new ClothingProductModel
        {
            Id = product.Id,
            Name = "Jacket Updated",
            Manufacturer = "BrandA"
        };

        var result = await service.UpdateProductAsync(model);

        Assert.True(result);
        var updated = await context.ClothingProducts.FindAsync(product.Id);
        Assert.Equal("Jacket Updated", updated!.Name);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldReturnFalseIfNotFound()
    {
        var context = GetDbContext();
        var service = new ClothingProductService(context, _mapper);

        var model = new ClothingProductModel
        {
            Id = Guid.NewGuid(),
            Name = "Jacket",
            Manufacturer = "BrandA"
        };

        var result = await service.UpdateProductAsync(model);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldThrowIfDuplicate()
    {
        var context = GetDbContext();
        var product1 = new ClothingProduct { Id = Guid.NewGuid(), Name = "Jacket", Manufacturer = "BrandA" };
        var product2 = new ClothingProduct { Id = Guid.NewGuid(), Name = "Pants", Manufacturer = "BrandB" };
        context.ClothingProducts.Add(product1);
        context.ClothingProducts.Add(product2);
        context.SaveChanges();

        var service = new ClothingProductService(context, _mapper);

        var model = new ClothingProductModel
        {
            Id = product2.Id,
            Name = "Jacket",
            Manufacturer = "BrandA"
        };

        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateProductAsync(model));
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldDeleteProduct()
    {
        var context = GetDbContext();
        var product = new ClothingProduct { Id = Guid.NewGuid(), Name = "Jacket", Manufacturer = "BrandA" };
        context.ClothingProducts.Add(product);
        context.SaveChanges();

        var service = new ClothingProductService(context, _mapper);

        var result = await service.DeleteProductAsync(product.Id);

        Assert.True(result);
        Assert.False(context.ClothingProducts.Any());
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldReturnFalseIfNotFound()
    {
        var context = GetDbContext();
        var service = new ClothingProductService(context, _mapper);

        var result = await service.DeleteProductAsync(Guid.NewGuid());

        Assert.False(result);
    }
}