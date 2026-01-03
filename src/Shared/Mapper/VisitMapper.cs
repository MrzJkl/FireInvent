using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class VisitMapper : BaseMapper
{
    public partial VisitModel MapVisitToVisitModel(Visit visit);

    [MapValue(nameof(Visit.Id), Use = nameof(NewGuid))]
    public partial Visit MapCreateOrUpdateVisitModelToVisit(CreateOrUpdateVisitModel createVisitModel);

    public partial List<VisitModel> MapVisitsToVisitModels(List<Visit> visits);

    public partial IQueryable<VisitModel> ProjectVisitsToVisitModels(IQueryable<Visit> visits);

    public partial void MapCreateOrUpdateVisitModelToVisit(CreateOrUpdateVisitModel source, Visit target);

    // VisitItem mappings
    public partial VisitItemModel MapVisitItemToVisitItemModel(VisitItem visitItem);

    [MapValue(nameof(VisitItem.Id), Use = nameof(NewGuid))]
    public partial VisitItem MapCreateOrUpdateVisitItemModelToVisitItem(CreateOrUpdateVisitItemModel createVisitItemModel);

    public partial List<VisitItemModel> MapVisitItemsToVisitItemModels(List<VisitItem> visitItems);

    public partial IQueryable<VisitItemModel> ProjectVisitItemsToVisitItemModels(IQueryable<VisitItem> visitItems);

    public partial void MapCreateOrUpdateVisitItemModelToVisitItem(CreateOrUpdateVisitItemModel source, VisitItem target);
}
