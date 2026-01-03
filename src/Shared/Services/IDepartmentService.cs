using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IDepartmentService
{
    Task<DepartmentModel> CreateDepartmentAsync(CreateOrUpdateDepartmentModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteDepartmentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<DepartmentModel>> GetAllDepartmentsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<DepartmentModel?> GetDepartmentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UpdateDepartmentAsync(Guid id, CreateOrUpdateDepartmentModel model, CancellationToken cancellationToken = default);
}