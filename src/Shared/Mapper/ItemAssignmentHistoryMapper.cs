using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class ItemAssignmentHistoryMapper : BaseMapper
{
    [MapperIgnoreTarget(nameof(ItemAssignmentHistoryModel.StockWarnings))]
    public partial ItemAssignmentHistoryModel MapItemAssignmentHistoryToItemAssignmentHistoryModel(ItemAssignmentHistory itemAssignmentHistory);

    [MapValue(nameof(ItemAssignmentHistory.Id), Use = nameof(NewGuid))]
    public partial ItemAssignmentHistory MapCreateOrUpdateItemAssignmentHistoryModelToItemAssignmentHistory(CreateOrUpdateItemAssignmentHistoryModel createItemAssignmentHistoryModel);

    [MapperIgnoreTarget(nameof(ItemAssignmentHistoryModel.StockWarnings))]
    public partial List<ItemAssignmentHistoryModel> MapItemAssignmentHistorysToItemAssignmentHistoryModels(List<ItemAssignmentHistory> itemAssignmentHistorys);

    [MapperIgnoreTarget(nameof(ItemAssignmentHistoryModel.StockWarnings))]
    public partial IQueryable<ItemAssignmentHistoryModel> ProjectItemAssignmentHistorysToItemAssignmentHistoryModels(IQueryable<ItemAssignmentHistory> itemAssignmentHistorys);

    public partial void MapCreateOrUpdateItemAssignmentHistoryModelToItemAssignmentHistory(CreateOrUpdateItemAssignmentHistoryModel source, ItemAssignmentHistory target);

    [MapperIgnoreTarget(nameof(StorageLocationModel.HasStockWarning))]
    private partial StorageLocationModel MapStorageLocationToStorageLocationModel(StorageLocation storageLocation);
}
