using FlameGuardLaundry.Shared.Models;
using FlameGuardLaundry.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlameGuardLaundry.Api.Controllers
{
    [ApiController]
    [Route("departments")]
    public class DepartmentsController(DepartmentService departmentService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<DepartmentModel>>> GetAll()
        {
            var departments = await departmentService.GetAllDepartmentsAsync();
            return Ok(departments);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<DepartmentModel>> GetById(Guid id)
        {
            var department = await departmentService.GetDepartmentByIdAsync(id);
            if (department is null)
                return NotFound();

            return Ok(department);
        }

        [HttpPost]
        public async Task<ActionResult<DepartmentModel>> Create(DepartmentModel model)
        {
            var created = await departmentService.CreateDepartmentAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, DepartmentModel model)
        {
            if (id != model.Id)
                return BadRequest("ID mismatch.");

            var success = await departmentService.UpdateDepartmentAsync(model);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await departmentService.DeleteDepartmentAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
