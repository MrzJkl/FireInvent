using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IStorageLocationService
{
    Task<StorageLocationModel> CreateStorageLocationAsync(CreateOrUpdateStorageLocationModel model);
    Task<bool> DeleteStorageLocationAsync(Guid id);
    Task<List<StorageLocationModel>> GetAllStorageLocationsAsync();
    Task<StorageLocationModel?> GetStorageLocationByIdAsync(Guid id);
    Task<bool> UpdateStorageLocationAsync(StorageLocationModel model);
}