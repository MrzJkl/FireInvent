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
    [ProducesResponseType<List<OrderModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OrderModel>>> GetAll()
    {
        var orders = await orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get order by ID")]
    [EndpointDescription("Returns an order by its unique ID.")]
    [ProducesResponseType<OrderModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderModel>> GetById(Guid id)
    {
        var order = await orderService.GetOrderByIdAsync(id);
        return order is null ? throw new NotFoundException() : Ok(order);
    }

    [HttpPost]
    [EndpointSummary("Create a new order")]
    [EndpointDescription("Creates a new order.")]
    [ProducesResponseType<OrderModel>(StatusCodes.Status201Created)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<ActionResult<OrderModel>> Create(CreateOrUpdateOrderModel model)
    {
        var created = await orderService.CreateOrderAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update an order")]
    [EndpointDescription("Updates an existing order.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateOrderModel model)
    {
        var success = await orderService.UpdateOrderAsync(id, model);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete an order")]
    [EndpointDescription("Deletes an order by its unique ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await orderService.DeleteOrderAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }
}
