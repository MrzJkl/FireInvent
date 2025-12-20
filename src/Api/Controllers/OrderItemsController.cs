using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("order-items")]
public class OrderItemsController(IOrderItemService orderItemService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List all order items")]
    [EndpointDescription("Returns a list of all order items.")]
    [ProducesResponseType<List<OrderItemModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OrderItemModel>>> GetAll()
    {
        var orderItems = await orderItemService.GetAllOrderItemsAsync();
        return Ok(orderItems);
    }

    [HttpGet("by-order/{orderId:guid}")]
    [EndpointSummary("List order items by order ID")]
    [EndpointDescription("Returns a list of order items for a specific order.")]
    [ProducesResponseType<List<OrderItemModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OrderItemModel>>> GetByOrderId(Guid orderId)
    {
        var orderItems = await orderItemService.GetOrderItemsByOrderIdAsync(orderId);
        return Ok(orderItems);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get order item by ID")]
    [EndpointDescription("Returns an order item by its unique ID.")]
    [ProducesResponseType<OrderItemModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderItemModel>> GetById(Guid id)
    {
        var orderItem = await orderItemService.GetOrderItemByIdAsync(id);
        return orderItem is null ? throw new NotFoundException() : Ok(orderItem);
    }

    [HttpPost]
    [EndpointSummary("Create a new order item")]
    [EndpointDescription("Creates a new order item.")]
    [ProducesResponseType<OrderItemModel>(StatusCodes.Status201Created)]
    public async Task<ActionResult<OrderItemModel>> Create(CreateOrUpdateOrderItemModel model)
    {
        var created = await orderItemService.CreateOrderItemAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update an order item")]
    [EndpointDescription("Updates an existing order item.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateOrderItemModel model)
    {
        var success = await orderItemService.UpdateOrderItemAsync(id, model);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete an order item")]
    [EndpointDescription("Deletes an order item by its unique ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await orderItemService.DeleteOrderItemAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }
}
