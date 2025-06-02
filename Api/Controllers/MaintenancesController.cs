using FlameGuardLaundry.Shared.Exceptions;
using FlameGuardLaundry.Shared.Models;
using FlameGuardLaundry.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FlameGuardLaundry.Api.Controllers
{
    [ApiController]
    [Route("maintenances")]
    public class MaintenancesController(MaintenanceService service) : ControllerBase
    {
        [HttpGet]
        [SwaggerOperation(Summary = "List all maintenances", Description = "Returns a list of all maintenance records.")]
        [SwaggerResponse(200, "List of maintenances", typeof(List<MaintenanceModel>))]
        public async Task<ActionResult<List<MaintenanceModel>>> GetAll()
        {
            var list = await service.GetAllMaintenancesAsync();
            return Ok(list);
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "Get maintenance by ID", Description = "Returns a maintenance record by its unique ID.")]
        [SwaggerResponse(200, "Maintenance found", typeof(MaintenanceModel))]
        [SwaggerResponse(404, "Maintenance not found")]
        public async Task<ActionResult<MaintenanceModel>> GetById(Guid id)
        {
            var result = await service.GetMaintenanceByIdAsync(id);
            return result is null ? throw new NotFoundException() : Ok(result);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new maintenance", Description = "Creates a new maintenance record.")]
        [SwaggerResponse(201, "Maintenance created", typeof(MaintenanceModel))]
        [SwaggerResponse(400, "Invalid input or referenced item does not exist")]
        public async Task<ActionResult<MaintenanceModel>> Create(MaintenanceModel model)
        {
            var created = await service.CreateMaintenanceAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(Summary = "Update a maintenance", Description = "Updates an existing maintenance record.")]
        [SwaggerResponse(204, "Maintenance updated")]
        [SwaggerResponse(400, "ID mismatch or referenced item does not exist")]
        [SwaggerResponse(404, "Maintenance not found")]
        public async Task<IActionResult> Update(Guid id, MaintenanceModel model)
        {
            if (id != model.Id)
                throw new IdMismatchException();

            var success = await service.UpdateMaintenanceAsync(model);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id:guid}")]
        [SwaggerOperation(Summary = "Delete a maintenance", Description = "Deletes a maintenance record by its unique ID.")]
        [SwaggerResponse(204, "Maintenance deleted")]
        [SwaggerResponse(404, "Maintenance not found")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await service.DeleteMaintenanceAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
