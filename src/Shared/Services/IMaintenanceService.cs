using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IMaintenanceService
{
    Task<MaintenanceModel> CreateMaintenanceAsync(CreateOrUpdateMaintenanceModel model);
    Task<bool> DeleteMaintenanceAsync(Guid id);
    Task<List<MaintenanceModel>> GetAllMaintenancesAsync();
    Task<MaintenanceModel?> GetMaintenanceByIdAsync(Guid id);
    Task<List<MaintenanceModel>> GetMaintenancesForItemAsync(Guid itemId);
    Task<bool> UpdateMaintenanceAsync(Guid id, CreateOrUpdateMaintenanceModel model);
}