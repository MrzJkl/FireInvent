using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services
{
    public interface IMaintenanceTypeService
    {
        Task<MaintenanceTypeModel> CreateMaintenanceTypeAsync(CreateOrUpdateMaintenanceTypeModel model);
        Task<bool> DeleteMaintenanceTypeAsync(Guid id);
        Task<PagedResult<MaintenanceTypeModel>> GetAllMaintenanceTypesAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
        Task<MaintenanceTypeModel?> GetMaintenanceTypeByIdAsync(Guid id);
        Task<bool> UpdateMaintenanceTypeAsync(Guid id, CreateOrUpdateMaintenanceTypeModel model);
    }
}