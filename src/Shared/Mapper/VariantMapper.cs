using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class VariantMapper : BaseMapper
{
    [MapperIgnoreSource(nameof(Variant.Product))]
    [MapperIgnoreSource(nameof(Variant.Items))]
    public partial VariantModel MapVariantToVariantModel(Variant variant);

    [MapValue(nameof(Variant.Id), Use = nameof(NewGuid))]
    [MapperIgnoreTarget(nameof(Variant.Product))]
    [MapperIgnoreTarget(nameof(Variant.Items))]
    public partial Variant MapCreateOrUpdateVariantModelToVariant(CreateOrUpdateVariantModel createVariantModel);

    public partial List<VariantModel> MapVariantsToVariantModels(List<Variant> variants);

    [MapperIgnoreTarget(nameof(Variant.Id))]
    [MapperIgnoreTarget(nameof(Variant.Product))]
    [MapperIgnoreTarget(nameof(Variant.Items))]
    public partial void MapCreateOrUpdateVariantModelToVariant(CreateOrUpdateVariantModel source, Variant target);
}
