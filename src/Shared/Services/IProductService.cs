using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IProductService
{
    Task<ProductModel> CreateProductAsync(CreateOrUpdateProductModel model);
    Task<bool> DeleteProductAsync(Guid id);
    Task<List<ProductModel>> GetAllProductsAsync();
    Task<ProductModel?> GetProductByIdAsync(Guid id);
    Task<bool> UpdateProductAsync(ProductModel model);
}