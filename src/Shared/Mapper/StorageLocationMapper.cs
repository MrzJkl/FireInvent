using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class StorageLocationMapper : BaseMapper
{
    public partial StorageLocationModel MapStorageLocationToStorageLocationModel(StorageLocation storageLocation);

    [MapValue(nameof(StorageLocation.Id), Use = nameof(NewGuid))]
    public partial StorageLocation MapCreateStorageLocationModelToStorageLocation(CreateStorageLocationModel createStorageLocationModel);

    public partial List<StorageLocationModel> MapStorageLocationsToStorageLocationModels(List<StorageLocation> storageLocations);

    [MapperIgnoreTarget(nameof(StorageLocation.Id))]
    public partial void MapStorageLocationModelToStorageLocation(StorageLocationModel source, StorageLocation target);
}
