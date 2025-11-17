using FireInvent.Contract;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("items")]
public class ItemsController(
    IItemService itemService,
    IItemAssignmentHistoryService assignmentHistoryService,
    IMaintenanceService maintenanceService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "List all items", Description = "Returns a list of all items.")]
    [SwaggerResponse(200, "List of items", typeof(List<ItemModel>))]
    public async Task<ActionResult<List<ItemModel>>> GetAll()
    {
        var items = await itemService.GetAllItemsAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get item by ID", Description = "Returns a item by its unique ID.")]
    [SwaggerResponse(200, "Item found", typeof(ItemModel))]
    [SwaggerResponse(404, "Item not found")]
    public async Task<ActionResult<ItemModel>> GetById(Guid id)
    {
        var item = await itemService.GetItemByIdAsync(id);
        return item is null ? throw new NotFoundException() : Ok(item);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new item", Description = "Creates a new item.")]
    [SwaggerResponse(201, "Item created", typeof(ItemModel))]
    [SwaggerResponse(400, "Invalid input or referenced variant/location not found")]
    [SwaggerResponse(409, "A item with the same identifier already exists")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<ActionResult<ItemModel>> Create(CreateOrUpdateItemModel model)
    {
        var created = await itemService.CreateItemAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Update a item", Description = "Updates an existing item.")]
    [SwaggerResponse(204, "Item updated")]
    [SwaggerResponse(400, "ID mismatch or referenced variant/location not found")]
    [SwaggerResponse(404, "Item not found")]
    [SwaggerResponse(409, "A item with the same identifier already exists")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateItemModel model)
    {
        if (id != model.Id)
            throw new IdMismatchException();

        var success = await itemService.UpdateItemAsync(model);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Delete a item", Description = "Deletes a item by its unique ID.")]
    [SwaggerResponse(204, "Item deleted")]
    [SwaggerResponse(404, "Item not found")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await itemService.DeleteItemAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpGet("{id:guid}/assignments")]
    [SwaggerOperation(Summary = "List all assignments for a item", Description = "Returns all assignment histories for a specific item.")]
    [SwaggerResponse(200, "List of assignment histories", typeof(List<ItemAssignmentHistoryModel>))]
    [SwaggerResponse(404, "Item not found")]
    public async Task<ActionResult<List<ItemAssignmentHistoryModel>>> GetAssignmentsForItem(Guid id)
    {
        var assignments = await assignmentHistoryService.GetAssignmentsForItemAsync(id);
        return Ok(assignments);
    }

    [HttpGet("{id:guid}/maintenance")]
    [SwaggerOperation(Summary = "List all maintenances for a item", Description = "Returns all maintenance records for a specific item.")]
    [SwaggerResponse(200, "List of maintenances", typeof(List<MaintenanceModel>))]
    [SwaggerResponse(404, "Item not found")]
    public async Task<ActionResult<List<MaintenanceModel>>> GetMaintenanceForItem(Guid id)
    {
        var maintenances = await maintenanceService.GetMaintenancesForItemAsync(id);
        return Ok(maintenances);
    }
}
