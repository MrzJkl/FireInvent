using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class ManufacturerMapper : BaseMapper
{
    public partial ManufacturerModel MapManufacturerToManufacturerModel(Manufacturer manufacturer);

    [MapValue(nameof(Manufacturer.Id), Use = nameof(NewGuid))]
    public partial Manufacturer MapCreateOrUpdateManufacturerModelToManufacturer(CreateOrUpdateManufacturerModel createManufacturerModel);



    public partial List<ManufacturerModel> MapManufacturersToManufacturerModels(List<Manufacturer> manufacturers);
    public partial IQueryable<ManufacturerModel> ProjectManufacturersToManufacturerModels(IQueryable<Manufacturer> manufacturers);

    public partial void MapCreateOrUpdateManufacturerModelToManufacturer(CreateOrUpdateManufacturerModel source, Manufacturer target);
}
