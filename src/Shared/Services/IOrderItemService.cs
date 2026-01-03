using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IOrderItemService
{
    Task<OrderItemModel> CreateOrderItemAsync(CreateOrUpdateOrderItemModel model);
    Task<bool> DeleteOrderItemAsync(Guid id);
    Task<PagedResult<OrderItemModel>> GetAllOrderItemsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<PagedResult<OrderItemModel>> GetOrderItemsByOrderIdAsync(Guid orderId, PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<OrderItemModel?> GetOrderItemByIdAsync(Guid id);
    Task<bool> UpdateOrderItemAsync(Guid id, CreateOrUpdateOrderItemModel model);
}
