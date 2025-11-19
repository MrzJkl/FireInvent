using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IOrderService
{
    Task<OrderModel> CreateOrderAsync(CreateOrUpdateOrderModel model);
    Task<bool> DeleteOrderAsync(Guid id);
    Task<List<OrderModel>> GetAllOrdersAsync();
    Task<OrderModel?> GetOrderByIdAsync(Guid id);
    Task<bool> UpdateOrderAsync(Guid id, CreateOrUpdateOrderModel model);
}