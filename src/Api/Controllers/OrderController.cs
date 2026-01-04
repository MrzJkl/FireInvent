using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("orders")]
public class OrderController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List all orders")]
    [EndpointDescription("Returns a list of all orders.")]
    [ProducesResponseType<PagedResult<OrderModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<OrderModel>>> GetAll(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var orders = await orderService.GetAllOrdersAsync(pagedQuery, cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get order by ID")]
    [EndpointDescription("Returns an order by its unique ID.")]
    [ProducesResponseType<OrderModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderModel>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await orderService.GetOrderByIdAsync(id, cancellationToken);
        return order is null ? throw new NotFoundException() : Ok(order);
    }

    [HttpPost]
    [EndpointSummary("Create a new order")]
    [EndpointDescription("Creates a new order.")]
    [ProducesResponseType<OrderModel>(StatusCodes.Status201Created)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<ActionResult<OrderModel>> Create(CreateOrUpdateOrderModel model, CancellationToken cancellationToken)
    {
        var created = await orderService.CreateOrderAsync(model, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update an order")]
    [EndpointDescription("Updates an existing order.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateOrderModel model, CancellationToken cancellationToken)
    {
        var success = await orderService.UpdateOrderAsync(id, model, cancellationToken);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete an order")]
    [EndpointDescription("Deletes an order by its unique ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var success = await orderService.DeleteOrderAsync(id, cancellationToken);
        return success ? NoContent() : throw new NotFoundException();
    }
}
