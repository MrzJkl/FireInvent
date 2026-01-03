using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IVariantService
{
    Task<VariantModel> CreateVariantAsync(CreateOrUpdateVariantModel model);
    Task<bool> DeleteVariantAsync(Guid id);
    Task<PagedResult<VariantModel>> GetAllVariantsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<VariantModel?> GetVariantByIdAsync(Guid id);
    Task<PagedResult<VariantModel>> GetVariantsForProductAsync(Guid productId, PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<bool> UpdateVariantAsync(Guid id, CreateOrUpdateVariantModel model);
}