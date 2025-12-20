using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IProductService
{
    Task<ProductModel> CreateProductAsync(CreateOrUpdateProductModel model);
    Task<bool> DeleteProductAsync(Guid id);
    Task<List<ProductModel>> GetAllProductsAsync();
    Task<ProductModel?> GetProductByIdAsync(Guid id);
    Task<List<ProductModel>> GetProductsForManufacturer(Guid manufacturerId);
    Task<bool> UpdateProductAsync(Guid id, CreateOrUpdateProductModel model);
}