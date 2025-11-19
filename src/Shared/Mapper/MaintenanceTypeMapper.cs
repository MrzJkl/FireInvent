using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class MaintenanceTypeMapper : BaseMapper
{
    public partial MaintenanceTypeModel MapMaintenanceTypeToMaintenanceTypeModel(MaintenanceType maintenanceType);

    [MapValue(nameof(MaintenanceType.Id), Use = nameof(NewGuid))]
    public partial MaintenanceType MapCreateOrUpdateMaintenanceTypeModelToMaintenanceType(CreateOrUpdateMaintenanceTypeModel createMaintenanceTypeModel);

    public partial List<MaintenanceTypeModel> MapMaintenanceTypesToMaintenanceTypeModels(List<MaintenanceType> maintenanceTypes);

    public partial void MapCreateOrUpdateMaintenanceTypeModelToMaintenanceType(CreateOrUpdateMaintenanceTypeModel source, MaintenanceType target, Guid id);
}
