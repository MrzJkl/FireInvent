using FlameGuardLaundry.Shared.Exceptions;
using FlameGuardLaundry.Shared.Models;
using FlameGuardLaundry.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlameGuardLaundry.Api.Controllers
{
    [ApiController]
    [Route("clothingItems")]
    public class ClothingItemsController(ClothingItemService itemService, ClothingItemAssignmentHistoryService assignmentHistoryService, MaintenanceService maintenanceService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<ClothingItemModel>>> GetAll()
        {
            var items = await itemService.GetAllClothingItemsAsync();
            return Ok(items);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ClothingItemModel>> GetById(Guid id)
        {
            var item = await itemService.GetClothingItemByIdAsync(id);
            return item is null ? throw new NotFoundException() : Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<ClothingItemModel>> Create(ClothingItemModel model)
        {
            var created = await itemService.CreateClothingItemAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, ClothingItemModel model)
        {
            if (id != model.Id)
                throw new IdMismatchException();

            var success = await itemService.UpdateClothingItemAsync(model);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await itemService.DeleteClothingItemAsync(id);
            return success ? NoContent() : NotFound();
        }

        [HttpGet("{id:guid}/assignments")]
        public async Task<ActionResult<List<ClothingItemAssignmentHistoryModel>>> GetAssignmentsForItem(Guid id)
        {
            var assignments = await assignmentHistoryService.GetAssignmentsForItemAsync(id);
            return Ok(assignments);
        }

        [HttpGet("{id:guid}/maintenance")]
        public async Task<ActionResult<List<MaintenanceModel>>> GetMaintenanceForItem(Guid id)
        {
            var maintenances = await maintenanceService.GetMaintenancesForItemAsync(id);
            return Ok(maintenances);
        }
    }
}
