using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IVariantService
{
    Task<VariantModel> CreateVariantAsync(CreateOrUpdateVariantModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteVariantAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<VariantModel>> GetAllVariantsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<VariantModel?> GetVariantByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<VariantModel>> GetVariantsForProductAsync(Guid productId, PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<bool> UpdateVariantAsync(Guid id, CreateOrUpdateVariantModel model, CancellationToken cancellationToken = default);
}