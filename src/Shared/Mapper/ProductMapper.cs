using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class ProductMapper : BaseMapper
{
    public partial ProductModel MapProductToProductModel(Product product);

    [MapValue(nameof(Product.Id), Use = nameof(NewGuid))]
    public partial Product MapCreateProductModelToProduct(CreateProductModel createProductModel);

    public partial List<ProductModel> MapProductsToProductModels(List<Product> products);

    [MapperIgnoreTarget(nameof(Product.Id))]
    [MapperIgnoreTarget(nameof(Product.Type))]
    public partial void MapProductModelToProduct(ProductModel source, Product target);
}
