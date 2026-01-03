using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Database;
using FireInvent.Database.Extensions;
using FireInvent.Shared.Extensions;
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

    public async Task<PagedResult<OrderItemModel>> GetAllOrderItemsAsync(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var query = context.OrderItems
            .OrderBy(oi => oi.OrderId)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectOrderItemsToOrderItemModels(query);

        return await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<OrderItemModel>> GetOrderItemsByOrderIdAsync(Guid orderId, PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var query = context.OrderItems
            .Where(oi => oi.OrderId == orderId)
            .OrderBy(oi => oi.CreatedAt)
            .AsNoTracking();

        query = query.ApplySearch(pagedQuery.SearchTerm);

        var projected = mapper.ProjectOrderItemsToOrderItemModels(query);

        return await projected.ToPagedResultAsync(
            pagedQuery.Page,
            pagedQuery.PageSize,
            cancellationToken);
    }

    public async Task<OrderItemModel> CreateOrderItemAsync(CreateOrUpdateOrderItemModel model)
    {
        _ = await context.Orders.FindAsync(model.OrderId)
            ?? throw new BadRequestException($"Order with ID '{model.OrderId}' does not exist.");
        _ = await context.Variants.FindAsync(model.VariantId)
            ?? throw new BadRequestException($"Variant with ID '{model.VariantId}' does not exist.");

        if (model.PersonId.HasValue)
        {
            _ = await context.Persons.FindAsync(model.PersonId.Value)
                ?? throw new BadRequestException($"Person with ID '{model.PersonId.Value}' does not exist.");
        }

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
        // Validate that Order exists
        _ = await context.Orders.FindAsync(model.OrderId)
            ?? throw new BadRequestException($"Order with ID '{model.OrderId}' does not exist.");

        // Validate that Variant exists
        _ = await context.Variants.FindAsync(model.VariantId)
            ?? throw new BadRequestException($"Variant with ID '{model.VariantId}' does not exist.");

        // Validate that Person exists (if PersonId is provided)
        if (model.PersonId.HasValue)
        {
            _ = await context.Persons.FindAsync(model.PersonId.Value)
                ?? throw new BadRequestException($"Person with ID '{model.PersonId.Value}' does not exist.");
        }

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
