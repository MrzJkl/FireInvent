using FireInvent.Contract;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("maintenanceTypes")]
public class MaintenanceTypesController(IMaintenanceTypeService maintenanceTypeService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "List all maintenanceTypes", Description = "Returns a list of all maintenanceTypes.")]
    [SwaggerResponse(200, "List of maintenanceTypes", typeof(List<MaintenanceTypeModel>))]
    public async Task<ActionResult<List<MaintenanceTypeModel>>> GetAll()
    {
        var maintenanceTypes = await maintenanceTypeService.GetAllMaintenanceTypesAsync();
        return Ok(maintenanceTypes);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get maintenanceType by ID", Description = "Returns a maintenanceType by its unique ID.")]
    [SwaggerResponse(200, "MaintenanceType found", typeof(MaintenanceTypeModel))]
    [SwaggerResponse(404, "MaintenanceType not found")]
    public async Task<ActionResult<MaintenanceTypeModel>> GetById(Guid id)
    {
        var maintenanceType = await maintenanceTypeService.GetMaintenanceTypeByIdAsync(id);
        return maintenanceType is null ? throw new NotFoundException() : (ActionResult<MaintenanceTypeModel>)Ok(maintenanceType);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new maintenanceType", Description = "Creates a new maintenanceType.")]
    [SwaggerResponse(201, "MaintenanceType created", typeof(MaintenanceTypeModel))]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<MaintenanceTypeModel>> Create(CreateOrUpdateMaintenanceTypeModel model)
    {
        var created = await maintenanceTypeService.CreateMaintenanceTypeAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Update a maintenanceType", Description = "Updates an existing maintenanceType.")]
    [SwaggerResponse(204, "MaintenanceType updated")]
    [SwaggerResponse(400, "ID mismatch")]
    [SwaggerResponse(404, "MaintenanceType not found")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateMaintenanceTypeModel model)
    {
        if (id != model.Id)
            throw new IdMismatchException();

        var success = await maintenanceTypeService.UpdateMaintenanceTypeAsync(model);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Delete a maintenanceType", Description = "Deletes a maintenanceType by its unique ID.")]
    [SwaggerResponse(204, "MaintenanceType deleted")]
    [SwaggerResponse(404, "MaintenanceType not found")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await maintenanceTypeService.DeleteMaintenanceTypeAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }
}
