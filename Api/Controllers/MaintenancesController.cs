using FlameGuardLaundry.Shared.Exceptions;
using FlameGuardLaundry.Shared.Models;
using FlameGuardLaundry.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlameGuardLaundry.Api.Controllers
{
    [ApiController]
    [Route("maintenances")]
    public class MaintenancesController(MaintenanceService service) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<MaintenanceModel>>> GetAll()
        {
            var list = await service.GetAllMaintenancesAsync();
            return Ok(list);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<MaintenanceModel>> GetById(Guid id)
        {
            var result = await service.GetMaintenanceByIdAsync(id);
            return result is null ? throw new NotFoundException() : Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<MaintenanceModel>> Create(MaintenanceModel model)
        {
            var created = await service.CreateMaintenanceAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, MaintenanceModel model)
        {
            if (id != model.Id)
                throw new IdMismatchException();

            var success = await service.UpdateMaintenanceAsync(model);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await service.DeleteMaintenanceAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
