using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("departments")]
public class DepartmentsController(IDepartmentService departmentService, IPersonService personService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List all departments")]
    [EndpointDescription("Returns a list of all departments.")]
    [ProducesResponseType<PagedResult<DepartmentModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<DepartmentModel>>> GetAll(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var departments = await departmentService.GetAllDepartmentsAsync(pagedQuery, cancellationToken);
        return Ok(departments);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get department by ID")]
    [EndpointDescription("Returns a department by its unique ID.")]
    [ProducesResponseType<DepartmentModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentModel>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var department = await departmentService.GetDepartmentByIdAsync(id, cancellationToken);
        return department is null ? throw new NotFoundException() : (ActionResult<DepartmentModel>)Ok(department);
    }

    [HttpPost]
    [EndpointSummary("Create a new department")]
    [EndpointDescription("Creates a new department.")]
    [ProducesResponseType<DepartmentModel>(StatusCodes.Status201Created)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<ActionResult<DepartmentModel>> Create(CreateOrUpdateDepartmentModel model, CancellationToken cancellationToken)
    {
        var created = await departmentService.CreateDepartmentAsync(model, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update a department")]
    [EndpointDescription("Updates an existing department.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateDepartmentModel model, CancellationToken cancellationToken)
    {
        var success = await departmentService.UpdateDepartmentAsync(id, model, cancellationToken);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete a department")]
    [EndpointDescription("Deletes a department by its unique ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var success = await departmentService.DeleteDepartmentAsync(id, cancellationToken);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpGet("{id:guid}/persons")]
    [EndpointSummary("List all persons in a department")]
    [EndpointDescription("Returns all persons that are members of the given department.")]
    [ProducesResponseType<PagedResult<PersonModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<PersonModel>>> GetPersonsForDepartment(Guid id, PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var persons = await personService.GetPersonsForDepartmentAsync(id, pagedQuery, cancellationToken);
        return Ok(persons);
    }
}
