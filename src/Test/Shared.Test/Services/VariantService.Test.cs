using FireInvent.Database;
using FireInvent.Database.Models;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;

namespace FireInvent.Test.Shared.Services;

public class VariantServiceTest
{
    private readonly VariantMapper _mapper;

    public VariantServiceTest()
    {
        _mapper = new VariantMapper();
    }

    private static Product CreateProduct(AppDbContext context)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Jacket",
            Manufacturer = "BrandA",
            Description = "Waterproof jacket",
            Type = Contract.ProductType.Jacket
        };
        context.Products.Add(product);
        context.SaveChanges();
        return product;
    }

    [Fact]
    public async Task CreateVariantAsync_ShouldCreateVariant()
    {
        var context = TestHelper.GetTestDbContext();
        var product = CreateProduct(context);
        var service = new VariantService(context, _mapper);

        var model = new CreateVariantModel
        {
            ProductId = product.Id,
            Name = "Red",
            AdditionalSpecs = "XL, Reflective"
        };

        var result = await service.CreateVariantAsync(model);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(model.ProductId, result.ProductId);
        Assert.Equal(model.Name, result.Name);
        Assert.Equal(model.AdditionalSpecs, result.AdditionalSpecs);

        var entity = await context.Variants.FindAsync(result.Id);
        Assert.NotNull(entity);
        Assert.Equal(model.ProductId, entity!.ProductId);
        Assert.Equal(model.Name, entity.Name);
        Assert.Equal(model.AdditionalSpecs, entity.AdditionalSpecs);
    }

    [Fact]
    public async Task CreateVariantAsync_ShouldThrowIfDuplicate()
    {
        var context = TestHelper.GetTestDbContext();
        var product = CreateProduct(context);
        context.Variants.Add(new Variant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Name = "Red",
            AdditionalSpecs = "XL, Reflective"
        });
        context.SaveChanges();

        var service = new VariantService(context, _mapper);

        var model = new CreateVariantModel
        {
            ProductId = product.Id,
            Name = "Red",
            AdditionalSpecs = "XL, Reflective"
        };

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateVariantAsync(model));
    }

    [Fact]
    public async Task CreateVariantAsync_ShouldThrowIfProductNotExists()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new VariantService(context, _mapper);

        var model = new CreateVariantModel
        {
            ProductId = Guid.NewGuid(),
            Name = "Red",
            AdditionalSpecs = "XL, Reflective"
        };

        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateVariantAsync(model));
    }

    [Fact]
    public async Task GetAllVariantsAsync_ShouldReturnAllVariants()
    {
        var context = TestHelper.GetTestDbContext();
        var product = CreateProduct(context);
        var variant1 = new Variant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Name = "Red",
            AdditionalSpecs = "XL, Reflective"
        };
        var variant2 = new Variant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Name = "Blue",
            AdditionalSpecs = "L, Non-reflective"
        };
        context.Variants.Add(variant1);
        context.Variants.Add(variant2);
        context.SaveChanges();

        var service = new VariantService(context, _mapper);

        var result = await service.GetAllVariantsAsync();

        Assert.Equal(2, result.Count);
        var red = result.FirstOrDefault(v => v.Name == "Red");
        var blue = result.FirstOrDefault(v => v.Name == "Blue");

        Assert.NotNull(red);
        Assert.Equal(variant1.Id, red!.Id);
        Assert.Equal(variant1.ProductId, red.ProductId);
        Assert.Equal(variant1.AdditionalSpecs, red.AdditionalSpecs);

        Assert.NotNull(blue);
        Assert.Equal(variant2.Id, blue!.Id);
        Assert.Equal(variant2.ProductId, blue.ProductId);
        Assert.Equal(variant2.AdditionalSpecs, blue.AdditionalSpecs);
    }

    [Fact]
    public async Task GetVariantByIdAsync_ShouldReturnVariant()
    {
        var context = TestHelper.GetTestDbContext();
        var product = CreateProduct(context);
        var variant = new Variant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Name = "Red",
            AdditionalSpecs = "XL, Reflective"
        };
        context.Variants.Add(variant);
        context.SaveChanges();

        var service = new VariantService(context, _mapper);

        var result = await service.GetVariantByIdAsync(variant.Id);

        Assert.NotNull(result);
        Assert.Equal(variant.Id, result!.Id);
        Assert.Equal(variant.ProductId, result.ProductId);
        Assert.Equal(variant.Name, result.Name);
        Assert.Equal(variant.AdditionalSpecs, result.AdditionalSpecs);
    }

    [Fact]
    public async Task GetVariantByIdAsync_ShouldReturnNullIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new VariantService(context, _mapper);

        var result = await service.GetVariantByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateVariantAsync_ShouldUpdateVariant()
    {
        var context = TestHelper.GetTestDbContext();
        var product = CreateProduct(context);
        var variant = new Variant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Name = "Red",
            AdditionalSpecs = "XL, Reflective"
        };
        context.Variants.Add(variant);
        context.SaveChanges();

        var service = new VariantService(context, _mapper);

        var model = new VariantModel
        {
            Id = variant.Id,
            ProductId = product.Id,
            Name = "Red Updated",
            AdditionalSpecs = "XXL, Reflective"
        };

        var result = await service.UpdateVariantAsync(model);

        Assert.True(result);
        var updated = await context.Variants.FindAsync(variant.Id);
        Assert.NotNull(updated);
        Assert.Equal(model.Name, updated!.Name);
        Assert.Equal(model.ProductId, updated.ProductId);
        Assert.Equal(model.AdditionalSpecs, updated.AdditionalSpecs);
    }

    [Fact]
    public async Task UpdateVariantAsync_ShouldReturnFalseIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var product = CreateProduct(context);
        var service = new VariantService(context, _mapper);

        var model = new VariantModel
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Name = "Red",
            AdditionalSpecs = "XL, Reflective"
        };

        var result = await service.UpdateVariantAsync(model);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateVariantAsync_ShouldThrowIfDuplicate()
    {
        var context = TestHelper.GetTestDbContext();
        var product = CreateProduct(context);
        var variant1 = new Variant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Name = "Red",
            AdditionalSpecs = "XL, Reflective"
        };
        var variant2 = new Variant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Name = "Blue",
            AdditionalSpecs = "L, Non-reflective"
        };
        context.Variants.Add(variant1);
        context.Variants.Add(variant2);
        context.SaveChanges();

        var service = new VariantService(context, _mapper);

        var model = new VariantModel
        {
            Id = variant2.Id,
            ProductId = product.Id,
            Name = "Red",
            AdditionalSpecs = "L, Non-reflective"
        };

        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateVariantAsync(model));
    }

    [Fact]
    public async Task DeleteVariantAsync_ShouldDeleteVariant()
    {
        var context = TestHelper.GetTestDbContext();
        var product = CreateProduct(context);
        var variant = new Variant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Name = "Red",
            AdditionalSpecs = "XL, Reflective"
        };
        context.Variants.Add(variant);
        context.SaveChanges();

        var service = new VariantService(context, _mapper);

        var result = await service.DeleteVariantAsync(variant.Id);

        Assert.True(result);
        Assert.False(context.Variants.Any());
    }

    [Fact]
    public async Task DeleteVariantAsync_ShouldReturnFalseIfNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new VariantService(context, _mapper);

        var result = await service.DeleteVariantAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task GetVariantsForProductAsync_ShouldReturnVariantsForProduct()
    {
        var context = TestHelper.GetTestDbContext();
        var product = CreateProduct(context);
        var otherProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Pants",
            Manufacturer = "BrandB",
            Description = "Fire-resistant pants",
            Type = Contract.ProductType.Pants
        };
        context.Products.Add(otherProduct);
        context.SaveChanges();

        var variant1 = new Variant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Name = "Red",
            AdditionalSpecs = "XL, Reflective"
        };
        var variant2 = new Variant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Name = "Blue",
            AdditionalSpecs = "L, Non-reflective"
        };
        var variantOther = new Variant
        {
            Id = Guid.NewGuid(),
            ProductId = otherProduct.Id,
            Name = "Green",
            AdditionalSpecs = "M, Non-reflective"
        };
        context.Variants.Add(variant1);
        context.Variants.Add(variant2);
        context.Variants.Add(variantOther);
        context.SaveChanges();

        var service = new VariantService(context, _mapper);

        var result = await service.GetVariantsForProductAsync(product.Id);

        Assert.Equal(2, result.Count);
        var red = result.FirstOrDefault(v => v.Name == "Red");
        var blue = result.FirstOrDefault(v => v.Name == "Blue");

        Assert.NotNull(red);
        Assert.Equal(variant1.Id, red!.Id);
        Assert.Equal(variant1.ProductId, red.ProductId);
        Assert.Equal(variant1.AdditionalSpecs, red.AdditionalSpecs);

        Assert.NotNull(blue);
        Assert.Equal(variant2.Id, blue!.Id);
        Assert.Equal(variant2.ProductId, blue.ProductId);
        Assert.Equal(variant2.AdditionalSpecs, blue.AdditionalSpecs);
    }

    [Fact]
    public async Task GetVariantsForProductAsync_ShouldThrowIfProductNotFound()
    {
        var context = TestHelper.GetTestDbContext();
        var service = new VariantService(context, _mapper);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetVariantsForProductAsync(Guid.NewGuid()));
    }
}