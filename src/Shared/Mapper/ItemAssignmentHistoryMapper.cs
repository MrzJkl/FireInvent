using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class ItemAssignmentHistoryMapper : BaseMapper
{
    [MapperIgnoreSource(nameof(ItemAssignmentHistory.Item))]
    [MapperIgnoreSource(nameof(ItemAssignmentHistory.Person))]
    [MapperIgnoreSource(nameof(ItemAssignmentHistory.AssignedBy))]
    public partial ItemAssignmentHistoryModel MapItemAssignmentHistoryToItemAssignmentHistoryModel(ItemAssignmentHistory itemAssignmentHistory);

    [MapValue(nameof(ItemAssignmentHistory.Id), Use = nameof(NewGuid))]
    [MapperIgnoreTarget(nameof(ItemAssignmentHistory.Item))]
    [MapperIgnoreTarget(nameof(ItemAssignmentHistory.Person))]
    [MapperIgnoreTarget(nameof(ItemAssignmentHistory.AssignedBy))]
    public partial ItemAssignmentHistory MapCreateOrUpdateItemAssignmentHistoryModelToItemAssignmentHistory(CreateOrUpdateItemAssignmentHistoryModel createItemAssignmentHistoryModel);

    public partial List<ItemAssignmentHistoryModel> MapItemAssignmentHistorysToItemAssignmentHistoryModels(List<ItemAssignmentHistory> itemAssignmentHistorys);
        
    [MapperIgnoreTarget(nameof(ItemAssignmentHistory.Id))]
    [MapperIgnoreTarget(nameof(ItemAssignmentHistory.Item))]
    [MapperIgnoreTarget(nameof(ItemAssignmentHistory.Person))]
    [MapperIgnoreTarget(nameof(ItemAssignmentHistory.AssignedBy))]
    public partial void MapCreateOrUpdateItemAssignmentHistoryModelToItemAssignmentHistory(CreateOrUpdateItemAssignmentHistoryModel source, ItemAssignmentHistory target, Guid id);
}
