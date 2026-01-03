using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("visits")]
public class VisitsController(IVisitService visitService, IVisitItemService visitItemService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List all visits")]
    [EndpointDescription("Returns a list of all visits.")]
    [ProducesResponseType<PagedResult<VisitModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<VisitModel>>> GetAll(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var visits = await visitService.GetAllVisitsAsync(pagedQuery, cancellationToken);
        return Ok(visits);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get visit by ID")]
    [EndpointDescription("Returns a visit by its unique ID.")]
    [ProducesResponseType<VisitModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VisitModel>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var visit = await visitService.GetVisitByIdAsync(id, cancellationToken);
        return visit is null ? throw new NotFoundException() : Ok(visit);
    }

    [HttpGet("{id:guid}/items")]
    [EndpointSummary("List all items for a visit")]
    [EndpointDescription("Returns all items associated with a specific visit.")]
    [ProducesResponseType<PagedResult<VisitItemModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<VisitItemModel>>> GetVisitItems(Guid id, PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var visitItems = await visitItemService.GetVisitItemsByVisitIdAsync(id, pagedQuery, cancellationToken);
        return Ok(visitItems);
    }

    [HttpPost]
    [EndpointSummary("Create a new visit")]
    [EndpointDescription("Creates a new visit. A person can only have one visit per appointment.")]
    [ProducesResponseType<VisitModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<ActionResult<VisitModel>> Create(CreateOrUpdateVisitModel model, CancellationToken cancellationToken)
    {
        var created = await visitService.CreateVisitAsync(model, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update a visit")]
    [EndpointDescription("Updates an existing visit. The combination of appointment and person must remain unique.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateVisitModel model, CancellationToken cancellationToken)
    {
        var success = await visitService.UpdateVisitAsync(id, model, cancellationToken);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete a visit")]
    [EndpointDescription("Deletes a visit by its unique ID. This will also delete all associated visit items.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var success = await visitService.DeleteVisitAsync(id, cancellationToken);
        return success ? NoContent() : throw new NotFoundException();
    }
}
