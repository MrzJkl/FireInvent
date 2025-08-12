using AutoMapper;
using FireInvent.Contract;
using FireInvent.Database;
using FireInvent.Database.Models;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class OrderService(GearDbContext context, IMapper mapper)
{
    public async Task<OrderModel?> GetOrderByIdAsync(Guid id)
    {
        var order = await context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.ClothingVariant)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        return order is null ? null : mapper.Map<OrderModel>(order);
    }

    public async Task<List<OrderModel>> GetAllOrdersAsync()
    {
        var orders = await context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.ClothingVariant)
            .AsNoTracking()
            .ToListAsync();

        return mapper.Map<List<OrderModel>>(orders);
    }

    public async Task<OrderModel> CreateOrderAsync(CreateOrderModel model)
    {
        var entity = mapper.Map<Order>(model);

        context.Orders.Add(entity);
        await context.SaveChangesAsync();

        return mapper.Map<OrderModel>(entity);
    }

    public async Task<bool> UpdateOrderAsync(OrderModel model)
    {
        var entity = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == model.Id);

        if (entity is null || entity.Status == OrderStatus.Completed)
            return false;

        mapper.Map(model, entity);

        context.OrderItems.RemoveRange(entity.Items);
        entity.Items = mapper.Map<List<OrderItem>>(model.Items);
        foreach (var item in entity.Items)
        {
            item.Id = Guid.NewGuid();
        }

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
