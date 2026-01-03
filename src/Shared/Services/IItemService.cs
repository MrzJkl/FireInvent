using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IItemService
{
    Task<ItemModel> CreateItemAsync(CreateOrUpdateItemModel model);
    Task<bool> DeleteItemAsync(Guid id);
    Task<PagedResult<ItemModel>> GetAllItemsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<ItemModel?> GetItemByIdAsync(Guid id);
    Task<PagedResult<ItemModel>> GetItemsForVariantAsync(Guid variantId, PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<bool> UpdateItemAsync(Guid id, CreateOrUpdateItemModel model);
}