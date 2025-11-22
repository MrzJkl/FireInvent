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

    [MapperIgnoreTarget(nameof(Maintenance.Id))]
    public partial void MapCreateOrUpdateMaintenanceModelToMaintenance(CreateOrUpdateMaintenanceModel source, Maintenance target);
}
