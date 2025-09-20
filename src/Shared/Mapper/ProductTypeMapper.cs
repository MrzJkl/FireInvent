using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class ProductTypeMapper : BaseMapper
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Mapper", "RMG020:Source member is not mapped to any target member", Justification = "Hiding is intended.")]
    public partial ProductTypeModel MapProductTypeToProductTypeModel(ProductType productType);

    [MapValue(nameof(ProductType.Id), Use = nameof(NewGuid))]
    public partial ProductType MapCreateProductTypeModelToProductType(CreateProductTypeModel createProductTypeModel);

    public partial List<ProductTypeModel> MapProductTypesToProductTypeModels(List<ProductType> productTypes);

    [MapperIgnoreTarget(nameof(ProductType.Id))]
    public partial void MapProductTypeModelToProductType(ProductTypeModel source, ProductType target);
}
