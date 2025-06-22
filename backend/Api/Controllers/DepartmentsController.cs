using FlameGuardLaundry.Shared.Exceptions;
using FlameGuardLaundry.Shared.Models;
using FlameGuardLaundry.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FlameGuardLaundry.Api.Controllers;

[ApiController]
[Route("departments")]
public class DepartmentsController(DepartmentService departmentService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "List all departments", Description = "Returns a list of all departments.")]
    [SwaggerResponse(200, "List of departments", typeof(List<DepartmentModel>))]
    public async Task<ActionResult<List<DepartmentModel>>> GetAll()
    {
        var departments = await departmentService.GetAllDepartmentsAsync();
        return Ok(departments);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get department by ID", Description = "Returns a department by its unique ID.")]
    [SwaggerResponse(200, "Department found", typeof(DepartmentModel))]
    [SwaggerResponse(404, "Department not found")]
    public async Task<ActionResult<DepartmentModel>> GetById(Guid id)
    {
        var department = await departmentService.GetDepartmentByIdAsync(id);
        return department is null ? throw new NotFoundException() : (ActionResult<DepartmentModel>)Ok(department);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new department", Description = "Creates a new department.")]
    [SwaggerResponse(201, "Department created", typeof(DepartmentModel))]
    public async Task<ActionResult<DepartmentModel>> Create(DepartmentModel model)
    {
        var created = await departmentService.CreateDepartmentAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Update a department", Description = "Updates an existing department.")]
    [SwaggerResponse(204, "Department updated")]
    [SwaggerResponse(400, "ID mismatch")]
    [SwaggerResponse(404, "Department not found")]
    public async Task<IActionResult> Update(Guid id, DepartmentModel model)
    {
        if (id != model.Id)
            throw new IdMismatchException();

        var success = await departmentService.UpdateDepartmentAsync(model);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Delete a department", Description = "Deletes a department by its unique ID.")]
    [SwaggerResponse(204, "Department deleted")]
    [SwaggerResponse(404, "Department not found")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await departmentService.DeleteDepartmentAsync(id);
        return success ? NoContent() : NotFound();
    }
}
