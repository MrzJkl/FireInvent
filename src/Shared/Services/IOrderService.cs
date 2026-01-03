using FireInvent.Contract;
using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IOrderService
{
    Task<OrderModel> CreateOrderAsync(CreateOrUpdateOrderModel model);
    Task<bool> DeleteOrderAsync(Guid id);
    Task<PagedResult<OrderModel>> GetAllOrdersAsync(PagedQuery pagedQuery, CancellationToken cancellationToken);
    Task<OrderModel?> GetOrderByIdAsync(Guid id);
    Task<bool> UpdateOrderAsync(Guid id, CreateOrUpdateOrderModel model);
}