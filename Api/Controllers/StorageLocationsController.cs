using FlameGuardLaundry.Shared.Exceptions;
using FlameGuardLaundry.Shared.Models;
using FlameGuardLaundry.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlameGuardLaundry.Api.Controllers
{
    [ApiController]
    [Route("storageLocations")]
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
            return location is null ? throw new NotFoundException() : (ActionResult<StorageLocationModel>)Ok(location);
        }

        [HttpPost]
        public async Task<ActionResult<StorageLocationModel>> Create(StorageLocationModel model)
        {
            var created = await locationService.CreateStorageLocationAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, StorageLocationModel model)
        {
            if (id != model.Id)
                throw new IdMismatchException();

            var success = await locationService.UpdateStorageLocationAsync(model);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await locationService.DeleteStorageLocationAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
