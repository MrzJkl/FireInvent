using FireInvent.Contract;
using FireInvent.Database;
using FireInvent.Database.Extensions;
using FireInvent.Shared.Extensions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class OrderService(AppDbContext context, OrderMapper mapper) : IOrderService
{
    public async Task<OrderModel?> GetOrderByIdAsync(Guid id)
    {
        var order = await context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        return order is null ? null : mapper.MapOrderToOrderModel(order);
    }

    public async Task<PagedResult<OrderModel>> GetAllOrdersAsync(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var query = context.Orders
            .OrderByDescending(o => o.OrderDate)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectOrdersToOrderModels(query);

        return await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);
    }

    public async Task<OrderModel> CreateOrderAsync(CreateOrUpdateOrderModel model)
    {
        var order = mapper.MapCreateOrUpdateOrderModelToOrder(model);

        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();

        order = await context.Orders
            .AsNoTracking()
            .FirstAsync(o => o.Id == order.Id);

        return mapper.MapOrderToOrderModel(order);
    }

    public async Task<bool> UpdateOrderAsync(Guid id, CreateOrUpdateOrderModel model)
    {
        var entity = await context.Orders
            .FirstOrDefaultAsync(o => o.Id == id);

        if (entity is null || entity.Status == OrderStatus.Completed)
            return false;

        mapper.MapCreateOrUpdateOrderModelToOrder(model, entity);

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteOrderAsync(Guid id)
    {
        var entity = await context.Orders
            .FirstOrDefaultAsync(o => o.Id == id);

        if (entity is null)
            return false;

        context.Orders.Remove(entity);
        await context.SaveChangesAsync();
        return true;
    }
}
