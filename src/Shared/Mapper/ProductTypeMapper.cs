using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class ProductTypeMapper : BaseMapper
{
    public partial ProductTypeModel MapProductTypeToProductTypeModel(ProductType productType);

    [MapValue(nameof(ProductType.Id), Use = nameof(NewGuid))]
    public partial ProductType MapCreateOrUpdateProductTypeModelToProductType(CreateOrUpdateProductTypeModel createProductTypeModel);

    public partial List<ProductTypeModel> MapProductTypesToProductTypeModels(List<ProductType> productTypes);

    public partial void MapCreateOrUpdateProductTypeModelToProductType(CreateOrUpdateProductTypeModel source, ProductType target, Guid id);
}
