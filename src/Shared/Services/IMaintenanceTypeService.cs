using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services
{
    public interface IMaintenanceTypeService
    {
        Task<MaintenanceTypeModel> CreateMaintenanceTypeAsync(CreateOrUpdateMaintenanceTypeModel model);
        Task<bool> DeleteMaintenanceTypeAsync(Guid id);
        Task<List<MaintenanceTypeModel>> GetAllMaintenanceTypesAsync();
        Task<MaintenanceTypeModel?> GetMaintenanceTypeByIdAsync(Guid id);
        Task<bool> UpdateMaintenanceTypeAsync(Guid id, CreateOrUpdateMaintenanceTypeModel model);
    }
}