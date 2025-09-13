using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IItemService
{
    Task<ItemModel> CreateClothingItemAsync(CreateItemModel model);
    Task<bool> DeleteClothingItemAsync(Guid id);
    Task<List<ItemModel>> GetAllClothingItemsAsync();
    Task<ItemModel?> GetClothingItemByIdAsync(Guid id);
    Task<List<ItemModel>> GetClothingItemsForStorageLocationAsync(Guid storageLocationId);
    Task<List<ItemModel>> GetItemsForVariantAsync(Guid variantId);
    Task<bool> UpdateClothingItemAsync(ItemModel model);
}