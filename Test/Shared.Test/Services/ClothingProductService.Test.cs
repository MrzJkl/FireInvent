using AutoMapper;
using FireInvent.Contract;
using FireInvent.Database.Models;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;

namespace FireInvent.Test.Shared.Services;

public class ClothingProductServiceTest
{
    private readonly IMapper _mapper;

    public ClothingProductServiceTest()
    {
        _mapper = TestHelper.GetMapper();
    }

    [Fact]
    public async Task CreateProductAsync_ShouldCreateProduct()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new ClothingProductService(context, _mapper);

        var model = new CreateClothingProductModel
        {
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            Type = GearType.Jacket
        };

        var result = await service.CreateProductAsync(model);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.Name, result.Name);
        Assert.Equal(model.Manufacturer, result.Manufacturer);
        Assert.Equal(model.Description, result.Description);
        Assert.Equal(model.Type, result.Type);

        var entity = await context.ClothingProducts.FindAsync(result.Id);
        Assert.NotNull(entity);
        Assert.Equal(model.Name, entity!.Name);
        Assert.Equal(model.Manufacturer, entity.Manufacturer);
        Assert.Equal(model.Description, entity.Description);
        Assert.Equal(model.Type, entity.Type);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldThrowIfDuplicate()
    {
        var context = TestHelper.GetTestDbContext();
        context.ClothingProducts.Add(new ClothingProduct
        {
            Id = Guid.NewGuid(),
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            Type = GearType.Jacket
        });
        context.SaveChanges();

        var service = new ClothingProductService(context, _mapper);

        var model = new CreateClothingProductModel
        {
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            Type = GearType.Jacket
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateProductAsync(model));
    }

    [Fact]
    public async Task GetAllProductsAsync_ShouldReturnAllProducts()
    {
        var context = TestHelper.GetTestDbContext();
        var product1 = new ClothingProduct
        {
            Id = Guid.NewGuid(),
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            Type = GearType.Jacket
        };
        var product2 = new ClothingProduct
        {
            Id = Guid.NewGuid(),
            Name = "Pants",
            Manufacturer = "BrandB",
            Description = "Fire-resistant pants",
            Type = GearType.Pants
        };
        context.ClothingProducts.Add(product1);
        context.ClothingProducts.Add(product2);
        context.SaveChanges();

        var service = new ClothingProductService(context, _mapper);

        var result = await service.GetAllProductsAsync();

        Assert.Equal(2, result.Count);
        var jacket = result.FirstOrDefault(p => p.Name == "Jacket");
        var pants = result.FirstOrDefault(p => p.Name == "Pants");

        Assert.NotNull(jacket);
        Assert.Equal(product1.Id, jacket!.Id);
        Assert.Equal(product1.Manufacturer, jacket.Manufacturer);
        Assert.Equal(product1.Description, jacket.Description);
        Assert.Equal(product1.Type, jacket.Type);

        Assert.NotNull(pants);
        Assert.Equal(product2.Id, pants!.Id);
        Assert.Equal(product2.Manufacturer, pants.Manufacturer);
        Assert.Equal(product2.Description, pants.Description);
        Assert.Equal(product2.Type, pants.Type);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnProduct()
    {
        var context = TestHelper.GetTestDbContext();
        var product = new ClothingProduct
        {
            Id = Guid.NewGuid(),
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            Type = GearType.Jacket
        };
        context.ClothingProducts.Add(product);
        context.SaveChanges();

        var service = new ClothingProductService(context, _mapper);

        var result = await service.GetProductByIdAsync(product.Id);

        Assert.NotNull(result);
        Assert.Equal(product.Id, result!.Id);
        Assert.Equal(product.Name, result.Name);
        Assert.Equal(product.Manufacturer, result.Manufacturer);
        Assert.Equal(product.Description, result.Description);
        Assert.Equal(product.Type, result.Type);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnNullIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new ClothingProductService(context, _mapper);

        var result = await service.GetProductByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldUpdateProduct()
    {
        var context = TestHelper.GetTestDbContext();
        var product = new ClothingProduct
        {
            Id = Guid.NewGuid(),
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            Type = GearType.Jacket
        };
        context.ClothingProducts.Add(product);
        context.SaveChanges();

        var service = new ClothingProductService(context, _mapper);

        var model = new ClothingProductModel
        {
            Id = product.Id,
            Name = "Jacket Updated",
            Manufacturer = "BrandA Updated",
            Description = "Updated description",
            Type = GearType.Pants
        };

        var result = await service.UpdateProductAsync(model);

        Assert.True(result);
        var updated = await context.ClothingProducts.FindAsync(product.Id);
        Assert.NotNull(updated);
        Assert.Equal(model.Name, updated!.Name);
        Assert.Equal(model.Manufacturer, updated.Manufacturer);
        Assert.Equal(model.Description, updated.Description);
        Assert.Equal(model.Type, updated.Type);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldReturnFalseIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new ClothingProductService(context, _mapper);

        var model = new ClothingProductModel
        {
            Id = Guid.NewGuid(),
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            Type = GearType.Jacket
        };

        var result = await service.UpdateProductAsync(model);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldThrowIfDuplicate()
    {
        var context = TestHelper.GetTestDbContext();
        var product1 = new ClothingProduct
        {
            Id = Guid.NewGuid(),
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            Type = GearType.Jacket
        };
        var product2 = new ClothingProduct
        {
            Id = Guid.NewGuid(),
            Name = "Pants",
            Manufacturer = "BrandB",
            Description = "Fire-resistant pants",
            Type = GearType.Pants
        };
        context.ClothingProducts.Add(product1);
        context.ClothingProducts.Add(product2);
        context.SaveChanges();

        var service = new ClothingProductService(context, _mapper);

        var model = new ClothingProductModel
        {
            Id = product2.Id,
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Fire-resistant pants",
            Type = GearType.Pants
        };

        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateProductAsync(model));
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldDeleteProduct()
    {
        var context = TestHelper.GetTestDbContext();
        var product = new ClothingProduct
        {
            Id = Guid.NewGuid(),
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            Type = GearType.Jacket
        };
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
        var context = TestHelper.GetTestDbContext();
        var service = new ClothingProductService(context, _mapper);

        var result = await service.DeleteProductAsync(Guid.NewGuid());

        Assert.False(result);
    }
}