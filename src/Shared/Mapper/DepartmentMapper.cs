using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class DepartmentMapper : BaseMapper
{
    public partial DepartmentModel MapDepartmentToDepartmentModel(Department department);

    [MapValue(nameof(Department.Id), Use = nameof(NewGuid))]
    public partial Department MapCreateOrUpdateDepartmentModelToDepartment(CreateOrUpdateDepartmentModel createDepartmentModel);

    public partial List<DepartmentModel> MapDepartmentsToDepartmentModels(List<Department> departments);

    public partial void MapCreateOrUpdateDepartmentModelToDepartment(CreateOrUpdateDepartmentModel source, Department target);
}
