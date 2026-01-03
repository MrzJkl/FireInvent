using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IProductService
{
    Task<ProductModel> CreateProductAsync(CreateOrUpdateProductModel model);
    Task<bool> DeleteProductAsync(Guid id);
    Task<PagedResult<ProductModel>> GetAllProductsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<ProductModel?> GetProductByIdAsync(Guid id);
    Task<PagedResult<ProductModel>> GetProductsForManufacturer(Guid manufacturerId, PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<bool> UpdateProductAsync(Guid id, CreateOrUpdateProductModel model);
}