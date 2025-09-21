using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services
{
    public interface IMaintenanceTypeService
    {
        Task<MaintenanceTypeModel> CreateMaintenanceTypeAsync(CreateMaintenanceTypeModel model);
        Task<bool> DeleteMaintenanceTypeAsync(Guid id);
        Task<List<MaintenanceTypeModel>> GetAllMaintenanceTypesAsync();
        Task<MaintenanceTypeModel?> GetMaintenanceTypeByIdAsync(Guid id);
        Task<bool> UpdateMaintenanceTypeAsync(MaintenanceTypeModel model);
    }
}