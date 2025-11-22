using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class MaintenanceMapper : BaseMapper
{
    [MapperIgnoreSource(nameof(Maintenance.Type))]
    [MapperIgnoreSource(nameof(Maintenance.Item))]
    [MapperIgnoreSource(nameof(Maintenance.PerformedBy))]
    public partial MaintenanceModel MapMaintenanceToMaintenanceModel(Maintenance maintenance);

    [MapValue(nameof(Maintenance.Id), Use = nameof(NewGuid))]
    [MapperIgnoreTarget(nameof(Maintenance.Type))]
    [MapperIgnoreTarget(nameof(Maintenance.Item))]
    [MapperIgnoreTarget(nameof(Maintenance.PerformedBy))]
    public partial Maintenance MapCreateOrUpdateMaintenanceModelToMaintenance(CreateOrUpdateMaintenanceModel createMaintenanceModel);

    public partial List<MaintenanceModel> MapMaintenancesToMaintenanceModels(List<Maintenance> maintenances);

    [MapperIgnoreTarget(nameof(Maintenance.Id))]
    [MapperIgnoreTarget(nameof(Maintenance.Type))]
    [MapperIgnoreTarget(nameof(Maintenance.Item))]
    [MapperIgnoreTarget(nameof(Maintenance.PerformedBy))]
    public partial void MapCreateOrUpdateMaintenanceModelToMaintenance(CreateOrUpdateMaintenanceModel source, Maintenance target);
}
