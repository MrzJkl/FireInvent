using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IItemAssignmentHistoryService
{
    Task<ItemAssignmentHistoryModel> CreateAssignmentAsync(CreateOrUpdateItemAssignmentHistoryModel model);
    Task<bool> DeleteAssignmentAsync(Guid id);
    Task<List<ItemAssignmentHistoryModel>> GetAllAssignmentsAsync();
    Task<ItemAssignmentHistoryModel?> GetAssignmentByIdAsync(Guid id);
    Task<List<ItemAssignmentHistoryModel>> GetAssignmentsForItemAsync(Guid itemId);
    Task<List<ItemAssignmentHistoryModel>> GetAssignmentsForPersonAsync(Guid personId);
    Task<List<ItemAssignmentHistoryModel>> GetAssignmentsForStorageLocationAsync(Guid storageLocationId);
    Task<bool> UpdateAssignmentAsync(Guid id, CreateOrUpdateItemAssignmentHistoryModel model);
}