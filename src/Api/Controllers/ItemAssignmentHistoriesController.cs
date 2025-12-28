using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("assignments")]
public class ItemAssignmentHistoriesController(IItemAssignmentHistoryService service) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List all item assignments")]
    [EndpointDescription("Returns a list of all item assignment histories.")]
    [ProducesResponseType<List<ItemAssignmentHistoryModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ItemAssignmentHistoryModel>>> GetAll()
    {
        var assignments = await service.GetAllAssignmentsAsync();
        return Ok(assignments);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get assignment by ID")]
    [EndpointDescription("Returns a item assignment history by its unique ID.")]
    [ProducesResponseType<ItemAssignmentHistoryModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemAssignmentHistoryModel>> GetById(Guid id)
    {
        var assignment = await service.GetAssignmentByIdAsync(id);
        return assignment is null ? throw new NotFoundException() : Ok(assignment);
    }

    [HttpPost]
    [EndpointSummary("Create a new assignment")]
    [EndpointDescription("Creates a new item assignment history.")]
    [ProducesResponseType<ItemAssignmentHistoryModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<ActionResult<ItemAssignmentHistoryModel>> Create(CreateOrUpdateItemAssignmentHistoryModel model)
    {
        var created = await service.CreateAssignmentAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update an assignment")]
    [EndpointDescription("Updates an existing item assignment history.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateItemAssignmentHistoryModel model)
    {
        var success = await service.UpdateAssignmentAsync(id, model);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete an assignment")]
    [EndpointDescription("Deletes a item assignment history by its unique ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await service.DeleteAssignmentAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }
}
