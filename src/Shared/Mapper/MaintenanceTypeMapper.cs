using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class MaintenanceTypeMapper : BaseMapper
{
    public partial MaintenanceTypeModel MapMaintenanceTypeToMaintenanceTypeModel(MaintenanceType maintenanceType);

    [MapValue(nameof(MaintenanceType.Id), Use = nameof(NewGuid))]
    public partial MaintenanceType MapCreateMaintenanceTypeModelToMaintenanceType(CreateMaintenanceTypeModel createMaintenanceTypeModel);

    public partial List<MaintenanceTypeModel> MapMaintenanceTypesToMaintenanceTypeModels(List<MaintenanceType> maintenanceTypes);

    [MapperIgnoreTarget(nameof(MaintenanceType.Id))]
    [MapperIgnoreTarget(nameof(Product.Type))]
    public partial void MapMaintenanceTypeModelToMaintenanceType(MaintenanceTypeModel source, MaintenanceType target);
}
