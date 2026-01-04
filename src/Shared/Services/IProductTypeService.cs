using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services
{
    public interface IProductTypeService
    {
        Task<ProductTypeModel> CreateProductTypeAsync(CreateOrUpdateProductTypeModel model, CancellationToken cancellationToken = default);
        Task<bool> DeleteProductTypeAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedResult<ProductTypeModel>> GetAllProductTypesAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
        Task<ProductTypeModel?> GetProductTypeByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> UpdateProductTypeAsync(Guid id, CreateOrUpdateProductTypeModel model, CancellationToken cancellationToken = default);
    }
}