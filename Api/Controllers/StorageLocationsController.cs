using FlameGuardLaundry.Shared.Models;
using FlameGuardLaundry.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlameGuardLaundry.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StorageLocationsController(StorageLocationService locationService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<StorageLocationModel>>> GetAll()
        {
            var locations = await locationService.GetAllStorageLocationsAsync();
            return Ok(locations);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<StorageLocationModel>> GetById(Guid id)
        {
            var location = await locationService.GetStorageLocationByIdAsync(id);
            if (location is null)
                return NotFound();

            return Ok(location);
        }

        [HttpPost]
        public async Task<ActionResult<StorageLocationModel>> Create(StorageLocationModel model)
        {
            try
            {
                var created = await locationService.CreateStorageLocationAsync(model);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, StorageLocationModel model)
        {
            if (id != model.Id)
                return BadRequest("ID mismatch.");

            try
            {
                var success = await locationService.UpdateStorageLocationAsync(model);
                return success ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await locationService.DeleteStorageLocationAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
