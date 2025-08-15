using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services
{
    public interface IClothingProductService
    {
        Task<ClothingProductModel> CreateProductAsync(CreateClothingProductModel model);
        Task<bool> DeleteProductAsync(Guid id);
        Task<List<ClothingProductModel>> GetAllProductsAsync();
        Task<ClothingProductModel?> GetProductByIdAsync(Guid id);
        Task<bool> UpdateProductAsync(ClothingProductModel model);
    }
}