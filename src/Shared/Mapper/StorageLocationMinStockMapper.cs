using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class StorageLocationMinStockMapper : BaseMapper
{
    [MapValue(nameof(StorageLocationMinStock.Id), Use = nameof(NewGuid))]
    public partial StorageLocationMinStock MapCreateOrUpdateStorageLocationMinStockModelToStorageLocationMinStock(
        CreateOrUpdateStorageLocationMinStockModel model);

    public partial void MapCreateOrUpdateStorageLocationMinStockModelToStorageLocationMinStock(
        CreateOrUpdateStorageLocationMinStockModel source,
        StorageLocationMinStock target);
}
