using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("order-items")]
public class OrderItemsController(IOrderItemService orderItemService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List all order items")]
    [EndpointDescription("Returns a list of all order items.")]
    [ProducesResponseType<PagedResult<OrderItemModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<OrderItemModel>>> GetAll(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var orderItems = await orderItemService.GetAllOrderItemsAsync(pagedQuery, cancellationToken);
        return Ok(orderItems);
    }

    [HttpGet("by-order/{orderId:guid}")]
    [EndpointSummary("List order items by order ID")]
    [EndpointDescription("Returns a list of order items for a specific order.")]
    [ProducesResponseType<PagedResult<OrderItemModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<OrderItemModel>>> GetByOrderId(Guid orderId, PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var orderItems = await orderItemService.GetOrderItemsByOrderIdAsync(orderId, pagedQuery, cancellationToken);
        return Ok(orderItems);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get order item by ID")]
    [EndpointDescription("Returns an order item by its unique ID.")]
    [ProducesResponseType<OrderItemModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderItemModel>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var orderItem = await orderItemService.GetOrderItemByIdAsync(id, cancellationToken);
        return orderItem is null ? throw new NotFoundException() : Ok(orderItem);
    }

    [HttpPost]
    [EndpointSummary("Create a new order item")]
    [EndpointDescription("Creates a new order item.")]
    [ProducesResponseType<OrderItemModel>(StatusCodes.Status201Created)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<ActionResult<OrderItemModel>> Create(CreateOrUpdateOrderItemModel model, CancellationToken cancellationToken)
    {
        var created = await orderItemService.CreateOrderItemAsync(model, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update an order item")]
    [EndpointDescription("Updates an existing order item.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateOrderItemModel model, CancellationToken cancellationToken)
    {
        var success = await orderItemService.UpdateOrderItemAsync(id, model, cancellationToken);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete an order item")]
    [EndpointDescription("Deletes an order item by its unique ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var success = await orderItemService.DeleteOrderItemAsync(id, cancellationToken);
        return success ? NoContent() : throw new NotFoundException();
    }
}
