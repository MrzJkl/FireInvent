using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class ProductMapper : BaseMapper
{
    public partial ProductModel MapProductToProductModel(Product product);

    [MapValue(nameof(Product.Id), Use = nameof(NewGuid))]
    public partial Product MapCreateOrUpdateProductModelToProduct(CreateOrUpdateProductModel createProductModel);

    public partial List<ProductModel> MapProductsToProductModels(List<Product> products);

    public partial void MapCreateOrUpdateProductModelToProduct(CreateOrUpdateProductModel source, Product target);
}
