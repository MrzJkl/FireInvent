using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("items")]
public class ItemsController(
    IItemService itemService,
    IItemAssignmentHistoryService assignmentHistoryService,
    IMaintenanceService maintenanceService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List all items")]
    [EndpointDescription("Returns a list of all items.")]
    [ProducesResponseType<PagedResult<ItemModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ItemModel>>> GetAll(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var items = await itemService.GetAllItemsAsync(pagedQuery, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get item by ID")]
    [EndpointDescription("Returns a item by its unique ID.")]
    [ProducesResponseType<ItemModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemModel>> GetById(Guid id)
    {
        var item = await itemService.GetItemByIdAsync(id);
        return item is null ? throw new NotFoundException() : Ok(item);
    }

    [HttpPost]
    [EndpointSummary("Create a new item")]
    [EndpointDescription("Creates a new item.")]
    [ProducesResponseType<ItemModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<ActionResult<ItemModel>> Create(CreateOrUpdateItemModel model)
    {
        var created = await itemService.CreateItemAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update a item")]
    [EndpointDescription("Updates an existing item.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateItemModel model)
    {
        var success = await itemService.UpdateItemAsync(id, model);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete a item")]
    [EndpointDescription("Deletes a item by its unique ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await itemService.DeleteItemAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpGet("{id:guid}/assignments")]
    [EndpointSummary("List all assignments for a item")]
    [EndpointDescription("Returns all assignment histories for a specific item.")]
    [ProducesResponseType<PagedResult<ItemAssignmentHistoryModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<ItemAssignmentHistoryModel>>> GetAssignmentsForItem(Guid id, PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var assignments = await assignmentHistoryService.GetAssignmentsForItemAsync(id, pagedQuery, cancellationToken);
        return Ok(assignments);
    }

    [HttpGet("{id:guid}/maintenance")]
    [EndpointSummary("List all maintenances for a item")]
    [EndpointDescription("Returns all maintenance records for a specific item.")]
    [ProducesResponseType<PagedResult<MaintenanceModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<MaintenanceModel>>> GetMaintenanceForItem(Guid id, PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var maintenances = await maintenanceService.GetMaintenancesForItemAsync(id, pagedQuery, cancellationToken);
        return Ok(maintenances);
    }
}
