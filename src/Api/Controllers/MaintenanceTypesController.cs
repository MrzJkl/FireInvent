using FireInvent.Contract;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("maintenanceTypes")]
public class MaintenanceTypesController(IMaintenanceTypeService maintenanceTypeService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List all maintenanceTypes")]
    [EndpointDescription("Returns a list of all maintenanceTypes.")]
    [ProducesResponseType<List<MaintenanceTypeModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MaintenanceTypeModel>>> GetAll()
    {
        var maintenanceTypes = await maintenanceTypeService.GetAllMaintenanceTypesAsync();
        return Ok(maintenanceTypes);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get maintenanceType by ID")]
    [EndpointDescription("Returns a maintenanceType by its unique ID.")]
    [ProducesResponseType<MaintenanceTypeModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MaintenanceTypeModel>> GetById(Guid id)
    {
        var maintenanceType = await maintenanceTypeService.GetMaintenanceTypeByIdAsync(id);
        return maintenanceType is null ? throw new NotFoundException() : (ActionResult<MaintenanceTypeModel>)Ok(maintenanceType);
    }

    [HttpPost]
    [EndpointSummary("Create a new maintenanceType")]
    [EndpointDescription("Creates a new maintenanceType.")]
    [ProducesResponseType<MaintenanceTypeModel>(StatusCodes.Status201Created)]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<MaintenanceTypeModel>> Create(CreateOrUpdateMaintenanceTypeModel model)
    {
        var created = await maintenanceTypeService.CreateMaintenanceTypeAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update a maintenanceType")]
    [EndpointDescription("Updates an existing maintenanceType.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateMaintenanceTypeModel model)
    {
        var success = await maintenanceTypeService.UpdateMaintenanceTypeAsync(id, model);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete a maintenanceType")]
    [EndpointDescription("Deletes a maintenanceType by its unique ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await maintenanceTypeService.DeleteMaintenanceTypeAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }
}
