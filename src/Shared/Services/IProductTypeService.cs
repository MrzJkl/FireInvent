using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services
{
    public interface IProductTypeService
    {
        Task<ProductTypeModel> CreateProductTypeAsync(CreateOrUpdateProductTypeModel model);
        Task<bool> DeleteProductTypeAsync(Guid id);
        Task<List<ProductTypeModel>> GetAllProductTypesAsync();
        Task<ProductTypeModel?> GetProductTypeByIdAsync(Guid id);
        Task<bool> UpdateProductTypeAsync(Guid id, CreateOrUpdateProductTypeModel model);
    }
}