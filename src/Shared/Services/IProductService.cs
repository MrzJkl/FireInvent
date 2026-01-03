using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IProductService
{
    Task<ProductModel> CreateProductAsync(CreateOrUpdateProductModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductModel>> GetAllProductsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<ProductModel?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ProductModel>> GetProductsForManufacturer(Guid manufacturerId, PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<bool> UpdateProductAsync(Guid id, CreateOrUpdateProductModel model, CancellationToken cancellationToken = default);
}