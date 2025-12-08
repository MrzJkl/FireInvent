using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface ITenantService
{
    Task<TenantModel> CreateTenantAsync(CreateOrUpdateTenantModel model);
    Task<bool> DeleteTenantAsync(Guid id);
    Task<List<TenantModel>> GetAllTenantsAsync();
    Task<TenantModel?> GetTenantByIdAsync(Guid id);
    Task<bool> UpdateTenantAsync(Guid id, CreateOrUpdateTenantModel model);
}
