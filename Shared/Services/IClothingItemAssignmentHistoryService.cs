using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services
{
    public interface IClothingItemAssignmentHistoryService
    {
        Task<ClothingItemAssignmentHistoryModel> CreateAssignmentAsync(CreateClothingItemAssignmentHistoryModel model);
        Task<bool> DeleteAssignmentAsync(Guid id);
        Task<List<ClothingItemAssignmentHistoryModel>> GetAllAssignmentsAsync();
        Task<ClothingItemAssignmentHistoryModel?> GetAssignmentByIdAsync(Guid id);
        Task<List<ClothingItemAssignmentHistoryModel>> GetAssignmentsForItemAsync(Guid itemId);
        Task<bool> UpdateAssignmentAsync(ClothingItemAssignmentHistoryModel model);
    }
}