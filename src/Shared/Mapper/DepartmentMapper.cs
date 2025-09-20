using FireInvent.Database.Models;
using FireInvent.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace FireInvent.Shared.Mapper;

[Mapper]
public partial class DepartmentMapper : BaseMapper
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Mapper", "RMG020:Source member is not mapped to any target member", Justification = "Hiding is intended.")]
    public partial DepartmentModel MapDepartmentToDepartmentModel(Department department);

    [MapValue(nameof(Department.Id), Use = nameof(NewGuid))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Mapper", "RMG012:Source member was not found for target member", Justification = "<Pending>")]
    public partial Department MapCreateDepartmentModelToDepartment(CreateDepartmentModel createDepartmentModel);

    public partial List<DepartmentModel> MapDepartmentsToDepartmentModels(List<Department> departments);

    [MapperIgnoreTarget(nameof(Department.Id))]
    public partial void MapDepartmentModelToDepartment(DepartmentModel source, Department target);
}
