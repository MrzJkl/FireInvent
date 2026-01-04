using FireInvent.Shared.Models;
using System.Threading;

namespace FireInvent.Shared.Services;

public interface ITenantService
{
    Task<TenantModel> CreateTenantAsync(CreateOrUpdateTenantModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteTenantAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<TenantModel>> GetAllTenantsAsync(CancellationToken cancellationToken = default);
    Task<TenantModel?> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UpdateTenantAsync(Guid id, CreateOrUpdateTenantModel model, CancellationToken cancellationToken = default);
}
