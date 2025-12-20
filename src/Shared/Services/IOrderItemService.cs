using FireInvent.Shared.Models;

namespace FireInvent.Shared.Services;

public interface IOrderItemService
{
    Task<OrderItemModel> CreateOrderItemAsync(CreateOrUpdateOrderItemModel model);
    Task<bool> DeleteOrderItemAsync(Guid id);
    Task<List<OrderItemModel>> GetAllOrderItemsAsync();
    Task<List<OrderItemModel>> GetOrderItemsByOrderIdAsync(Guid orderId);
    Task<OrderItemModel?> GetOrderItemByIdAsync(Guid id);
    Task<bool> UpdateOrderItemAsync(Guid id, CreateOrUpdateOrderItemModel model);
}
