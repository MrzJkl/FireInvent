using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IStorageLocationService
{
    Task<StorageLocationModel> CreateStorageLocationAsync(CreateOrUpdateStorageLocationModel model);
    Task<bool> DeleteStorageLocationAsync(Guid id);
    Task<PagedResult<StorageLocationModel>> GetAllStorageLocationsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<StorageLocationModel?> GetStorageLocationByIdAsync(Guid id);
    Task<bool> UpdateStorageLocationAsync(Guid id, CreateOrUpdateStorageLocationModel model);
}