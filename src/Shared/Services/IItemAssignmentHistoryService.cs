using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IItemAssignmentHistoryService
{
    Task<ItemAssignmentHistoryModel> CreateAssignmentAsync(CreateOrUpdateItemAssignmentHistoryModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteAssignmentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ItemAssignmentHistoryModel>> GetAllAssignmentsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<ItemAssignmentHistoryModel?> GetAssignmentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ItemAssignmentHistoryModel>> GetAssignmentsForItemAsync(Guid itemId, PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<PagedResult<ItemAssignmentHistoryModel>> GetAssignmentsForPersonAsync(Guid personId, PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<PagedResult<ItemAssignmentHistoryModel>> GetAssignmentsForStorageLocationAsync(Guid storageLocationId, PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<bool> UpdateAssignmentAsync(Guid id, CreateOrUpdateItemAssignmentHistoryModel model, CancellationToken cancellationToken = default);
}