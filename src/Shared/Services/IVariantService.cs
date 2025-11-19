using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IVariantService
{
    Task<VariantModel> CreateVariantAsync(CreateOrUpdateVariantModel model);
    Task<bool> DeleteVariantAsync(Guid id);
    Task<List<VariantModel>> GetAllVariantsAsync();
    Task<VariantModel?> GetVariantByIdAsync(Guid id);
    Task<List<VariantModel>> GetVariantsForProductAsync(Guid productId);
    Task<bool> UpdateVariantAsync(Guid id, CreateOrUpdateVariantModel model);
}