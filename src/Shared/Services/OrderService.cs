using FireInvent.Contract;
using FireInvent.Database;
using FireInvent.Database.Extensions;
using FireInvent.Database.Models;
using FireInvent.Shared.Extensions;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services.Telemetry;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Services;

public class OrderService(AppDbContext context, OrderMapper mapper, FireInventTelemetry telemetry) : IOrderService
{
    public async Task<OrderModel?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var activity = telemetry.StartActivity("OrderService.GetOrderById");
        activity?.SetTag("order.id", id);

        var order = await context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        return order is null ? null : mapper.MapOrderToOrderModel(order);
    }

    public async Task<PagedResult<OrderModel>> GetAllOrdersAsync(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        using var activity = telemetry.StartActivity("OrderService.GetAllOrders");
        activity?.SetTag("page", pagedQuery.Page);
        activity?.SetTag("pageSize", pagedQuery.PageSize);

        var query = context.Orders
            .OrderByDescending(o => o.OrderDate)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectOrdersToOrderModels(query);

        var result = await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);

        activity?.SetTag("orders.count", result.TotalItems);

        return result;
    }

    public async Task<OrderModel> CreateOrderAsync(CreateOrUpdateOrderModel model, CancellationToken cancellationToken = default)
    {
        using var activity = telemetry.StartActivity("OrderService.CreateOrder");
        activity?.SetTag("order.identifier", model.OrderIdentifier);
        activity?.SetTag("order.status", model.Status);

        var order = mapper.MapCreateOrUpdateOrderModelToOrder(model);

        await context.Orders.AddAsync(order, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        order = await context.Orders
            .AsNoTracking()
            .FirstAsync(o => o.Id == order.Id, cancellationToken);

        // Record telemetry
        telemetry.OrdersCreatedCounter.Add(1,
            new KeyValuePair<string, object?>("order.status", model.Status.ToString()));

        activity?.SetTag("order.id", order.Id);
        activity?.SetTag("order.date", order.OrderDate);

        return mapper.MapOrderToOrderModel(order);
    }

    public async Task<bool> UpdateOrderAsync(Guid id, CreateOrUpdateOrderModel model, CancellationToken cancellationToken = default)
    {
        var entity = await context.Orders.FindAsync(id, cancellationToken);
        if (entity is null)
            return false;

        mapper.MapCreateOrUpdateOrderModelToOrder(model, entity);

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.TryDeleteEntityAsync(
            id,
            nameof(Order),
            context.Orders,
            cancellationToken);
    }
}
