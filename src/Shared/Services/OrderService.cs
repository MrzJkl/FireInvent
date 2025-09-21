using FireInvent.Contract;
using FireInvent.Database;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class OrderService(AppDbContext context, OrderMapper mapper) : IOrderService
{
    public async Task<OrderModel?> GetOrderByIdAsync(Guid id)
    {
        var order = await context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Variant)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        return order is null ? null : mapper.MapOrderToOrderModel(order);
    }

    public async Task<List<OrderModel>> GetAllOrdersAsync()
    {
        var orders = await context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Variant)
            .AsNoTracking()
            .ToListAsync();

        return mapper.MapOrdersToOrderModels(orders);
    }

    public async Task<OrderModel> CreateOrderAsync(CreateOrderModel model)
    {
        var entity = mapper.MapCreateOrderModelToOrder(model);

        context.Orders.Add(entity);
        await context.SaveChangesAsync();

        return mapper.MapOrderToOrderModel(entity);
    }

    public async Task<bool> UpdateOrderAsync(OrderModel model)
    {
        var entity = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == model.Id);

        if (entity is null || entity.Status == OrderStatus.Completed)
            return false;

        mapper.MapOrderModelToOrder(model, entity);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteOrderAsync(Guid id)
    {
        var entity = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (entity is null)
            return false;

        context.OrderItems.RemoveRange(entity.Items);
        context.Orders.Remove(entity);
        await context.SaveChangesAsync();
        return true;
    }
}
