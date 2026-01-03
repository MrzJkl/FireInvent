using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IVisitItemService
{
    Task<VisitItemModel> CreateVisitItemAsync(CreateOrUpdateVisitItemModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteVisitItemAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<VisitItemModel>> GetAllVisitItemsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<PagedResult<VisitItemModel>> GetVisitItemsByVisitIdAsync(Guid visitId, PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<VisitItemModel?> GetVisitItemByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UpdateVisitItemAsync(Guid id, CreateOrUpdateVisitItemModel model, CancellationToken cancellationToken = default);
}
