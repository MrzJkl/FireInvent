using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IVisitItemService
{
    Task<VisitItemModel> CreateVisitItemAsync(CreateOrUpdateVisitItemModel model);
    Task<bool> DeleteVisitItemAsync(Guid id);
    Task<List<VisitItemModel>> GetAllVisitItemsAsync();
    Task<List<VisitItemModel>> GetVisitItemsByVisitIdAsync(Guid visitId);
    Task<VisitItemModel?> GetVisitItemByIdAsync(Guid id);
    Task<bool> UpdateVisitItemAsync(Guid id, CreateOrUpdateVisitItemModel model);
}
