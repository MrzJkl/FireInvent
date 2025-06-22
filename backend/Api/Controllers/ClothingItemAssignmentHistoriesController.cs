using FlameGuardLaundry.Shared.Exceptions;
using FlameGuardLaundry.Shared.Models;
using FlameGuardLaundry.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FlameGuardLaundry.Api.Controllers;

[ApiController]
[Route("assignments")]
public class ClothingItemAssignmentHistoriesController(ClothingItemAssignmentHistoryService service) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "List all clothing item assignments", Description = "Returns a list of all clothing item assignment histories.")]
    [SwaggerResponse(200, "List of clothing item assignment histories", typeof(List<ClothingItemAssignmentHistoryModel>))]
    public async Task<ActionResult<List<ClothingItemAssignmentHistoryModel>>> GetAll()
    {
        var assignments = await service.GetAllAssignmentsAsync();
        return Ok(assignments);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get assignment by ID", Description = "Returns a clothing item assignment history by its unique ID.")]
    [SwaggerResponse(200, "Assignment found", typeof(ClothingItemAssignmentHistoryModel))]
    [SwaggerResponse(404, "Assignment not found")]
    public async Task<ActionResult<ClothingItemAssignmentHistoryModel>> GetById(Guid id)
    {
        var assignment = await service.GetAssignmentByIdAsync(id);
        return assignment is null ? throw new NotFoundException() : Ok(assignment);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new assignment", Description = "Creates a new clothing item assignment history.")]
    [SwaggerResponse(201, "Assignment created", typeof(ClothingItemAssignmentHistoryModel))]
    [SwaggerResponse(400, "Invalid input or referenced item/person does not exist")]
    [SwaggerResponse(409, "An overlapping assignment already exists for this item")]
    public async Task<ActionResult<ClothingItemAssignmentHistoryModel>> Create(CreateClothingItemAssignmentHistoryModel model)
    {
        var created = await service.CreateAssignmentAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Update an assignment", Description = "Updates an existing clothing item assignment history.")]
    [SwaggerResponse(204, "Assignment updated")]
    [SwaggerResponse(400, "ID mismatch or referenced item/person does not exist")]
    [SwaggerResponse(404, "Assignment not found")]
    [SwaggerResponse(409, "An overlapping assignment already exists for this item")]
    public async Task<IActionResult> Update(Guid id, ClothingItemAssignmentHistoryModel model)
    {
        if (id != model.Id)
            throw new IdMismatchException();

        var success = await service.UpdateAssignmentAsync(model);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Delete an assignment", Description = "Deletes a clothing item assignment history by its unique ID.")]
    [SwaggerResponse(204, "Assignment deleted")]
    [SwaggerResponse(404, "Assignment not found")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await service.DeleteAssignmentAsync(id);
        return success ? NoContent() : NotFound();
    }
}
