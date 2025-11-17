using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class ItemAssignmentHistoryMapper : BaseMapper
{
    public partial ItemAssignmentHistoryModel MapItemAssignmentHistoryToItemAssignmentHistoryModel(ItemAssignmentHistory itemAssignmentHistory);

    [MapValue(nameof(ItemAssignmentHistory.Id), Use = nameof(NewGuid))]
    public partial ItemAssignmentHistory MapCreateOrUpdateItemAssignmentHistoryModelToItemAssignmentHistory(CreateOrUpdateItemAssignmentHistoryModel createItemAssignmentHistoryModel);

    public partial List<ItemAssignmentHistoryModel> MapItemAssignmentHistorysToItemAssignmentHistoryModels(List<ItemAssignmentHistory> itemAssignmentHistorys);

    public partial void MapCreateOrUpdateItemAssignmentHistoryModelToItemAssignmentHistory(CreateOrUpdateItemAssignmentHistoryModel source, ItemAssignmentHistory target, Guid id);
}
