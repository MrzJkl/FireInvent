using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class VariantMapper : BaseMapper
{
    public partial VariantModel MapVariantToVariantModel(Variant variant);

    [MapValue(nameof(Variant.Id), Use = nameof(NewGuid))]
    public partial Variant MapCreateVariantModelToVariant(CreateVariantModel createVariantModel);

    public partial List<VariantModel> MapVariantsToVariantModels(List<Variant> variants);

    [MapperIgnoreTarget(nameof(Variant.Id))]
    public partial void MapVariantModelToVariant(VariantModel source, Variant target);
}
