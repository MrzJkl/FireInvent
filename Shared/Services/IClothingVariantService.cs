using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services
{
    public interface IClothingVariantService
    {
        Task<ClothingVariantModel> CreateVariantAsync(CreateClothingVariantModel model);
        Task<bool> DeleteVariantAsync(Guid id);
        Task<List<ClothingVariantModel>> GetAllVariantsAsync();
        Task<ClothingVariantModel?> GetVariantByIdAsync(Guid id);
        Task<List<ClothingVariantModel>> GetVariantsForProductAsync(Guid productId);
        Task<bool> UpdateVariantAsync(ClothingVariantModel model);
    }
}