using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("visit-items")]
public class VisitItemsController(IVisitItemService visitItemService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List all visit items")]
    [EndpointDescription("Returns a list of all visit items.")]
    [ProducesResponseType<List<VisitItemModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VisitItemModel>>> GetAll()
    {
        var visitItems = await visitItemService.GetAllVisitItemsAsync();
        return Ok(visitItems);
    }

    [HttpGet("by-visit/{visitId:guid}")]
    [EndpointSummary("List visit items by visit ID")]
    [EndpointDescription("Returns a list of visit items for a specific visit.")]
    [ProducesResponseType<List<VisitItemModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VisitItemModel>>> GetByVisitId(Guid visitId)
    {
        var visitItems = await visitItemService.GetVisitItemsByVisitIdAsync(visitId);
        return Ok(visitItems);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get visit item by ID")]
    [EndpointDescription("Returns a visit item by its unique ID.")]
    [ProducesResponseType<VisitItemModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VisitItemModel>> GetById(Guid id)
    {
        var visitItem = await visitItemService.GetVisitItemByIdAsync(id);
        return visitItem is null ? throw new NotFoundException() : Ok(visitItem);
    }

    [HttpPost]
    [EndpointSummary("Create a new visit item")]
    [EndpointDescription("Creates a new visit item.")]
    [ProducesResponseType<VisitItemModel>(StatusCodes.Status201Created)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<ActionResult<VisitItemModel>> Create(CreateOrUpdateVisitItemModel model)
    {
        var created = await visitItemService.CreateVisitItemAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update a visit item")]
    [EndpointDescription("Updates an existing visit item.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateVisitItemModel model)
    {
        var success = await visitItemService.UpdateVisitItemAsync(id, model);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete a visit item")]
    [EndpointDescription("Deletes a visit item by its unique ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await visitItemService.DeleteVisitItemAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }
}
