using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class MaintenanceMapper : BaseMapper
{
    public partial MaintenanceModel MapMaintenanceToMaintenanceModel(Maintenance Maintenance);

    [MapValue(nameof(Maintenance.Id), Use = nameof(NewGuid))]
    public partial Maintenance MapCreateMaintenanceModelToMaintenance(CreateMaintenanceModel createMaintenanceModel);

    public partial List<MaintenanceModel> MapMaintenancesToMaintenanceModels(List<Maintenance> Maintenances);

    [MapperIgnoreTarget(nameof(Maintenance.Id))]
    public partial void MapMaintenanceModelToMaintenance(MaintenanceModel source, Maintenance target);
}
