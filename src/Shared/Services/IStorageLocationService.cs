using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IStorageLocationService
{
    Task<StorageLocationModel> CreateStorageLocationAsync(CreateOrUpdateStorageLocationModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteStorageLocationAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<StorageLocationModel>> GetAllStorageLocationsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<StorageLocationModel?> GetStorageLocationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UpdateStorageLocationAsync(Guid id, CreateOrUpdateStorageLocationModel model, CancellationToken cancellationToken = default);
}