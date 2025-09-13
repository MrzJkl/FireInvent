using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class ItemAssignmentHistoryMapper : BaseMapper
{
    public partial ItemAssignmentHistoryModel MapItemAssignmentHistoryToItemAssignmentHistoryModel(ItemAssignmentHistory ItemAssignmentHistory);

    [MapValue(nameof(ItemAssignmentHistory.Id), Use = nameof(NewGuid))]
    public partial ItemAssignmentHistory MapCreateItemAssignmentHistoryModelToItemAssignmentHistory(CreateItemAssignmentHistoryModel createItemAssignmentHistoryModel);

    public partial List<ItemAssignmentHistoryModel> MapItemAssignmentHistorysToItemAssignmentHistoryModels(List<ItemAssignmentHistory> ItemAssignmentHistorys);

    [MapperIgnoreTarget(nameof(ItemAssignmentHistory.Id))]
    public partial void MapItemAssignmentHistoryModelToItemAssignmentHistory(ItemAssignmentHistoryModel source, ItemAssignmentHistory target);
}
