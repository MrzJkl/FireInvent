using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class ProductMapper : BaseMapper
{
    [MapperIgnoreSource(nameof(Product.Type))]
    [MapperIgnoreSource(nameof(Product.Variants))]
    public partial ProductModel MapProductToProductModel(Product product);

    [MapValue(nameof(Product.Id), Use = nameof(NewGuid))]
    [MapperIgnoreTarget(nameof(Product.Type))]
    [MapperIgnoreTarget(nameof(Product.Variants))]
    public partial Product MapCreateOrUpdateProductModelToProduct(CreateOrUpdateProductModel createProductModel);

    public partial List<ProductModel> MapProductsToProductModels(List<Product> products);

    [MapperIgnoreTarget(nameof(Product.Id))]
    [MapperIgnoreTarget(nameof(Product.Type))]
    [MapperIgnoreTarget(nameof(Product.Variants))]
    public partial void MapCreateOrUpdateProductModelToProduct(CreateOrUpdateProductModel source, Product target);
}
