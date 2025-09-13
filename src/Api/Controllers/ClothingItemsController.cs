using FireInvent.Contract;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("clothingItems")]
public class ClothingItemsController(
    IItemService itemService,
    IItemAssignmentHistoryService assignmentHistoryService,
    IMaintenanceService maintenanceService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "List all clothing items", Description = "Returns a list of all clothing items.")]
    [SwaggerResponse(200, "List of clothing items", typeof(List<ItemModel>))]
    public async Task<ActionResult<List<ItemModel>>> GetAll()
    {
        var items = await itemService.GetAllClothingItemsAsync();
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get clothing item by ID", Description = "Returns a clothing item by its unique ID.")]
    [SwaggerResponse(200, "Clothing item found", typeof(ItemModel))]
    [SwaggerResponse(404, "Clothing item not found")]
    public async Task<ActionResult<ItemModel>> GetById(Guid id)
    {
        var item = await itemService.GetClothingItemByIdAsync(id);
        return item is null ? throw new NotFoundException() : Ok(item);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new clothing item", Description = "Creates a new clothing item.")]
    [SwaggerResponse(201, "Clothing item created", typeof(ItemModel))]
    [SwaggerResponse(400, "Invalid input or referenced variant/location not found")]
    [SwaggerResponse(409, "A clothing item with the same identifier already exists")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<ActionResult<ItemModel>> Create(CreateItemModel model)
    {
        var created = await itemService.CreateClothingItemAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Update a clothing item", Description = "Updates an existing clothing item.")]
    [SwaggerResponse(204, "Clothing item updated")]
    [SwaggerResponse(400, "ID mismatch or referenced variant/location not found")]
    [SwaggerResponse(404, "Clothing item not found")]
    [SwaggerResponse(409, "A clothing item with the same identifier already exists")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Update(Guid id, ItemModel model)
    {
        if (id != model.Id)
            throw new IdMismatchException();

        var success = await itemService.UpdateClothingItemAsync(model);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Delete a clothing item", Description = "Deletes a clothing item by its unique ID.")]
    [SwaggerResponse(204, "Clothing item deleted")]
    [SwaggerResponse(404, "Clothing item not found")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await itemService.DeleteClothingItemAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpGet("{id:guid}/assignments")]
    [SwaggerOperation(Summary = "List all assignments for a clothing item", Description = "Returns all assignment histories for a specific clothing item.")]
    [SwaggerResponse(200, "List of assignment histories", typeof(List<ItemAssignmentHistoryModel>))]
    [SwaggerResponse(404, "Clothing item not found")]
    public async Task<ActionResult<List<ItemAssignmentHistoryModel>>> GetAssignmentsForItem(Guid id)
    {
        var assignments = await assignmentHistoryService.GetAssignmentsForItemAsync(id);
        return Ok(assignments);
    }

    [HttpGet("{id:guid}/maintenance")]
    [SwaggerOperation(Summary = "List all maintenances for a clothing item", Description = "Returns all maintenance records for a specific clothing item.")]
    [SwaggerResponse(200, "List of maintenances", typeof(List<MaintenanceModel>))]
    [SwaggerResponse(404, "Clothing item not found")]
    public async Task<ActionResult<List<MaintenanceModel>>> GetMaintenanceForItem(Guid id)
    {
        var maintenances = await maintenanceService.GetMaintenancesForItemAsync(id);
        return Ok(maintenances);
    }
}
