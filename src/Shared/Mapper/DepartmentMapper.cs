using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class DepartmentMapper : BaseMapper
{
    [MapperIgnoreSource(nameof(Department.Persons))]
    public partial DepartmentModel MapDepartmentToDepartmentModel(Department department);

    [MapValue(nameof(Department.Id), Use = nameof(NewGuid))]
    [MapperIgnoreTarget(nameof(Department.Persons))]
    public partial Department MapCreateOrUpdateDepartmentModelToDepartment(CreateOrUpdateDepartmentModel createDepartmentModel);

    public partial List<DepartmentModel> MapDepartmentsToDepartmentModels(List<Department> departments);

    [MapperIgnoreTarget(nameof(Department.Persons))]
    [MapperIgnoreTarget(nameof(Department.Id))]
    public partial void MapCreateOrUpdateDepartmentModelToDepartment(CreateOrUpdateDepartmentModel source, Department target);
}
