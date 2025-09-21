using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IOrderService
{
    Task<OrderModel> CreateOrderAsync(CreateOrderModel model);
    Task<bool> DeleteOrderAsync(Guid id);
    Task<List<OrderModel>> GetAllOrdersAsync();
    Task<OrderModel?> GetOrderByIdAsync(Guid id);
    Task<bool> UpdateOrderAsync(OrderModel model);
}