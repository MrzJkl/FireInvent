using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services
{
    public interface IClothingItemService
    {
        Task<ClothingItemModel> CreateClothingItemAsync(CreateClothingItemModel model);
        Task<bool> DeleteClothingItemAsync(Guid id);
        Task<List<ClothingItemModel>> GetAllClothingItemsAsync();
        Task<ClothingItemModel?> GetClothingItemByIdAsync(Guid id);
        Task<List<ClothingItemModel>> GetClothingItemsForStorageLocationAsync(Guid storageLocationId);
        Task<List<ClothingItemModel>> GetItemsForVariantAsync(Guid variantId);
        Task<bool> UpdateClothingItemAsync(ClothingItemModel model);
    }
}