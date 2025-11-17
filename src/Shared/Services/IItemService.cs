using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IItemService
{
    Task<ItemModel> CreateItemAsync(CreateOrUpdateItemModel model);
    Task<bool> DeleteItemAsync(Guid id);
    Task<List<ItemModel>> GetAllItemsAsync();
    Task<ItemModel?> GetItemByIdAsync(Guid id);
    Task<List<ItemModel>> GetItemsAssignedToPersonAsync(Guid personId);
    Task<List<ItemModel>> GetItemsForStorageLocationAsync(Guid storageLocationId);
    Task<List<ItemModel>> GetItemsForVariantAsync(Guid variantId);
    Task<bool> UpdateItemAsync(Guid id, CreateOrUpdateItemModel model);
}