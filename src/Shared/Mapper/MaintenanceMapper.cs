using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class MaintenanceMapper : BaseMapper
{
    public partial MaintenanceModel MapMaintenanceToMaintenanceModel(Maintenance maintenance);

    [MapValue(nameof(Maintenance.Id), Use = nameof(NewGuid))]
    public partial Maintenance MapCreateOrUpdateMaintenanceModelToMaintenance(CreateOrUpdateMaintenanceModel createMaintenanceModel);


    public partial List<MaintenanceModel> MapMaintenancesToMaintenanceModels(List<Maintenance> maintenances);

    public partial IQueryable<MaintenanceModel> ProjectMaintenancesToMaintenanceModels(IQueryable<Maintenance> maintenances);

    public partial void MapCreateOrUpdateMaintenanceModelToMaintenance(CreateOrUpdateMaintenanceModel source, Maintenance target);
}
