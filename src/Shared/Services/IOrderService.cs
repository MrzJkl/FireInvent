using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IOrderService
{
    Task<OrderModel> CreateOrderAsync(CreateOrUpdateOrderModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteOrderAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<OrderModel>> GetAllOrdersAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<OrderModel?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UpdateOrderAsync(Guid id, CreateOrUpdateOrderModel model, CancellationToken cancellationToken = default);
}