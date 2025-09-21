using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IItemAssignmentHistoryService
{
    Task<ItemAssignmentHistoryModel> CreateAssignmentAsync(CreateItemAssignmentHistoryModel model);
    Task<bool> DeleteAssignmentAsync(Guid id);
    Task<List<ItemAssignmentHistoryModel>> GetAllAssignmentsAsync();
    Task<ItemAssignmentHistoryModel?> GetAssignmentByIdAsync(Guid id);
    Task<List<ItemAssignmentHistoryModel>> GetAssignmentsForItemAsync(Guid itemId);
    Task<bool> UpdateAssignmentAsync(ItemAssignmentHistoryModel model);
}