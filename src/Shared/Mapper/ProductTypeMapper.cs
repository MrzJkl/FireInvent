using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class ProductTypeMapper : BaseMapper
{
    public partial ProductTypeModel MapProductTypeToProductTypeModel(ProductType productType);

    [MapValue(nameof(ProductType.Id), Use = nameof(NewGuid))]
    public partial ProductType MapCreateProductTypeModelToProductType(CreateOrUpdateProductTypeModel createProductTypeModel);

    public partial List<ProductTypeModel> MapProductTypesToProductTypeModels(List<ProductType> productTypes);

    [MapperIgnoreTarget(nameof(ProductType.Id))]
    public partial void MapProductTypeModelToProductType(ProductTypeModel source, ProductType target);
}
