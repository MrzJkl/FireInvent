using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class VariantMapper : BaseMapper
{
    public partial VariantModel MapVariantToVariantModel(Variant variant);

    [MapValue(nameof(Variant.Id), Use = nameof(NewGuid))]
    public partial Variant MapCreateOrUpdateVariantModelToVariant(CreateOrUpdateVariantModel createVariantModel);

    public partial List<VariantModel> MapVariantsToVariantModels(List<Variant> variants);

    public partial IQueryable<VariantModel> ProjectVariantsToVariantModels(IQueryable<Variant> variants);

    public partial void MapCreateOrUpdateVariantModelToVariant(CreateOrUpdateVariantModel source, Variant target);
}
