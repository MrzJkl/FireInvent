using FlameGuardLaundry.Shared.Exceptions;
using FlameGuardLaundry.Shared.Models;
using FlameGuardLaundry.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlameGuardLaundry.Api.Controllers
{
    [ApiController]
    [Route("assignments")]
    public class ClothingItemAssignmentHistoriesController(ClothingItemAssignmentHistoryService service) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<ClothingItemAssignmentHistoryModel>>> GetAll()
        {
            var assignments = await service.GetAllAssignmentsAsync();
            return Ok(assignments);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ClothingItemAssignmentHistoryModel>> GetById(Guid id)
        {
            var assignment = await service.GetAssignmentByIdAsync(id);
            return assignment is null ? throw new NotFoundException() : Ok(assignment);
        }

        [HttpPost]
        public async Task<ActionResult<ClothingItemAssignmentHistoryModel>> Create(ClothingItemAssignmentHistoryModel model)
        {
            var created = await service.CreateAssignmentAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, ClothingItemAssignmentHistoryModel model)
        {
            if (id != model.Id)
                throw new IdMismatchException();

            var success = await service.UpdateAssignmentAsync(model);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await service.DeleteAssignmentAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
