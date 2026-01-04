using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IOrderItemService
{
    Task<OrderItemModel> CreateOrderItemAsync(CreateOrUpdateOrderItemModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteOrderItemAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<OrderItemModel>> GetAllOrderItemsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<PagedResult<OrderItemModel>> GetOrderItemsByOrderIdAsync(Guid orderId, PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<OrderItemModel?> GetOrderItemByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UpdateOrderItemAsync(Guid id, CreateOrUpdateOrderItemModel model, CancellationToken cancellationToken = default);
}
