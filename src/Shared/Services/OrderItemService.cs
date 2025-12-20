using FireInvent.Contract.Exceptions;
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
