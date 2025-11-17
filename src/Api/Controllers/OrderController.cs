using FireInvent.Contract;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("orders")]
public class OrderController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "List all orders", Description = "Returns a list of all orders.")]
    [SwaggerResponse(200, "List of orders", typeof(List<OrderModel>))]
    public async Task<ActionResult<List<OrderModel>>> GetAll()
    {
        var orders = await orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get order by ID", Description = "Returns an order by its unique ID.")]
    [SwaggerResponse(200, "Order found", typeof(OrderModel))]
    [SwaggerResponse(404, "Order not found")]
    public async Task<ActionResult<OrderModel>> GetById(Guid id)
    {
        var order = await orderService.GetOrderByIdAsync(id);
        return order is null ? throw new NotFoundException() : Ok(order);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new order", Description = "Creates a new order.")]
    [SwaggerResponse(201, "Order created", typeof(OrderModel))]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<ActionResult<OrderModel>> Create(CreateOrUpdateOrderModel model)
    {
        var created = await orderService.CreateOrderAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Update an order", Description = "Updates an existing order.")]
    [SwaggerResponse(204, "Order updated")]
    [SwaggerResponse(400, "ID mismatch")]
    [SwaggerResponse(404, "Order not found or cannot be updated")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateOrderModel model)
    {
        if (id != model.Id)
            throw new IdMismatchException();

        var success = await orderService.UpdateOrderAsync(model);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Delete an order", Description = "Deletes an order by its unique ID.")]
    [SwaggerResponse(204, "Order deleted")]
    [SwaggerResponse(404, "Order not found.")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await orderService.DeleteOrderAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }
}
