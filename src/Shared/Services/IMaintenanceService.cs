using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IMaintenanceService
{
    Task<MaintenanceModel> CreateMaintenanceAsync(CreateOrUpdateMaintenanceModel model);
    Task<bool> DeleteMaintenanceAsync(Guid id);
    Task<PagedResult<MaintenanceModel>> GetAllMaintenancesAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<MaintenanceModel?> GetMaintenanceByIdAsync(Guid id);
    Task<PagedResult<MaintenanceModel>> GetMaintenancesForItemAsync(Guid itemId, PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<bool> UpdateMaintenanceAsync(Guid id, CreateOrUpdateMaintenanceModel model);
}