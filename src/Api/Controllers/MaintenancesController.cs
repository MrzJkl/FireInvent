using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("maintenances")]
public class MaintenancesController(IMaintenanceService service) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List all maintenances")]
    [EndpointDescription("Returns a list of all maintenance records.")]
    [ProducesResponseType<PagedResult<MaintenanceModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<MaintenanceModel>>> GetAll(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var maintenances = await service.GetAllMaintenancesAsync(pagedQuery, cancellationToken);
        return Ok(maintenances);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get maintenance by ID")]
    [EndpointDescription("Returns a maintenance record by its unique ID.")]
    [ProducesResponseType<MaintenanceModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MaintenanceModel>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.GetMaintenanceByIdAsync(id, cancellationToken);
        return result is null ? throw new NotFoundException() : Ok(result);
    }

    [HttpPost]
    [EndpointSummary("Create a new maintenance")]
    [EndpointDescription("Creates a new maintenance record.")]
    [ProducesResponseType<MaintenanceModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Maintenance + "," + Roles.Integration)]
    public async Task<ActionResult<MaintenanceModel>> Create(CreateOrUpdateMaintenanceModel model, CancellationToken cancellationToken)
    {
        var created = await service.CreateMaintenanceAsync(model, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update a maintenance")]
    [EndpointDescription("Updates an existing maintenance record.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Maintenance + "," + Roles.Integration)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateMaintenanceModel model, CancellationToken cancellationToken)
    {
        var success = await service.UpdateMaintenanceAsync(id, model, cancellationToken);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete a maintenance")]
    [EndpointDescription("Deletes a maintenance record by its unique ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Maintenance + "," + Roles.Integration)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var success = await service.DeleteMaintenanceAsync(id, cancellationToken);
        return success ? NoContent() : throw new NotFoundException();
    }
}
