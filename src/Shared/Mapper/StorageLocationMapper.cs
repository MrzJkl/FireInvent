using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class StorageLocationMapper : BaseMapper
{
    [MapperIgnoreTarget(nameof(StorageLocationModel.HasStockWarning))]
    public partial StorageLocationModel MapStorageLocationToStorageLocationModel(StorageLocation storageLocation);

    [MapValue(nameof(StorageLocation.Id), Use = nameof(NewGuid))]
    public partial StorageLocation MapCreateOrUpdateStorageLocationModelToStorageLocation(CreateOrUpdateStorageLocationModel createStorageLocationModel);

    [MapperIgnoreTarget(nameof(StorageLocationModel.HasStockWarning))]
    public partial List<StorageLocationModel> MapStorageLocationsToStorageLocationModels(List<StorageLocation> storageLocations);

    [MapperIgnoreTarget(nameof(StorageLocationModel.HasStockWarning))]
    public partial IQueryable<StorageLocationModel> ProjectStorageLocationsToStorageLocationModels(IQueryable<StorageLocation> storageLocations);

    public partial void MapCreateOrUpdateStorageLocationModelToStorageLocation(CreateOrUpdateStorageLocationModel source, StorageLocation target);
}
