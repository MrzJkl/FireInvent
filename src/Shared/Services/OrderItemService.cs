using FireInvent.Database;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class OrderItemService(AppDbContext context, OrderMapper mapper) : IOrderItemService
{
    public async Task<OrderItemModel?> GetOrderItemByIdAsync(Guid id)
    {
        var orderItem = await context.OrderItems
            .AsNoTracking()
            .FirstOrDefaultAsync(oi => oi.Id == id);

        return orderItem is null ? null : mapper.MapOrderItemToOrderItemModel(orderItem);
    }

    public async Task<List<OrderItemModel>> GetAllOrderItemsAsync()
    {
        var orderItems = await context.OrderItems
            .AsNoTracking()
            .ToListAsync();

        return mapper.MapOrderItemsToOrderItemModels(orderItems);
    }

    public async Task<List<OrderItemModel>> GetOrderItemsByOrderIdAsync(Guid orderId)
    {
        var orderItems = await context.OrderItems
            .AsNoTracking()
            .Where(oi => oi.OrderId == orderId)
            .ToListAsync();

        return mapper.MapOrderItemsToOrderItemModels(orderItems);
    }

    public async Task<OrderItemModel> CreateOrderItemAsync(CreateOrUpdateOrderItemModel model)
    {
        var orderItem = mapper.MapCreateOrUpdateOrderItemModelToOrderItem(model);

        await context.OrderItems.AddAsync(orderItem);
        await context.SaveChangesAsync();

        orderItem = await context.OrderItems
            .AsNoTracking()
            .FirstAsync(oi => oi.Id == orderItem.Id);

        return mapper.MapOrderItemToOrderItemModel(orderItem);
    }

    public async Task<bool> UpdateOrderItemAsync(Guid id, CreateOrUpdateOrderItemModel model)
    {
        var entity = await context.OrderItems
            .FirstOrDefaultAsync(oi => oi.Id == id);

        if (entity is null)
            return false;

        mapper.MapCreateOrUpdateOrderItemModelToOrderItem(model, entity);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteOrderItemAsync(Guid id)
    {
        var entity = await context.OrderItems
            .FirstOrDefaultAsync(oi => oi.Id == id);

        if (entity is null)
            return false;

        context.OrderItems.Remove(entity);
        await context.SaveChangesAsync();
        return true;
    }
}
