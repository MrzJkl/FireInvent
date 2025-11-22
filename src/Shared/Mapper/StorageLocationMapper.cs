using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class StorageLocationMapper : BaseMapper
{
    [MapperIgnoreSource(nameof(StorageLocation.StoredItems))]
    public partial StorageLocationModel MapStorageLocationToStorageLocationModel(StorageLocation storageLocation);

    [MapValue(nameof(StorageLocation.Id), Use = nameof(NewGuid))]
    [MapperIgnoreTarget(nameof(StorageLocation.StoredItems))]
    public partial StorageLocation MapCreateOrUpdateStorageLocationModelToStorageLocation(CreateOrUpdateStorageLocationModel createStorageLocationModel);

    public partial List<StorageLocationModel> MapStorageLocationsToStorageLocationModels(List<StorageLocation> storageLocations);

    [MapperIgnoreTarget(nameof(StorageLocation.StoredItems))]
    [MapperIgnoreTarget(nameof(StorageLocation.Id))]
    public partial void MapCreateOrUpdateStorageLocationModelToStorageLocation(CreateOrUpdateStorageLocationModel source, StorageLocation target);
}
