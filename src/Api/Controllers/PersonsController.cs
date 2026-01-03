using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("persons")]
public class PersonsController(IPersonService personService, IItemAssignmentHistoryService itemAssignmentHistoryService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List all persons")]
    [EndpointDescription("Returns a list of all persons.")]
    [ProducesResponseType<PagedResult<PersonModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PersonModel>>> GetAll(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var persons = await personService.GetAllPersonsAsync(pagedQuery, cancellationToken);
        return Ok(persons);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get person by ID")]
    [EndpointDescription("Returns a single person by their unique ID.")]
    [ProducesResponseType<PersonModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonModel>> GetById(Guid id)
    {
        var person = await personService.GetPersonByIdAsync(id);
        return person is null ? throw new NotFoundException() : (ActionResult<PersonModel>)Ok(person);
    }

    [HttpPost]
    [EndpointSummary("Create a new person")]
    [EndpointDescription("Creates a new person entry.")]
    [ProducesResponseType<PersonModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<ActionResult<PersonModel>> Create(CreateOrUpdatePersonModel model)
    {
        var created = await personService.CreatePersonAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update a person")]
    [EndpointDescription("Updates an existing person.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdatePersonModel model)
    {
        var success = await personService.UpdatePersonAsync(id, model);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete a person")]
    [EndpointDescription("Deletes a person by ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await personService.DeletePersonAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpGet("{id:guid}/assignments")]
    [EndpointSummary("List all assignments for a person")]
    [EndpointDescription("Returns all assignments for a specific person.")]
    [ProducesResponseType<PagedResult<ItemAssignmentHistoryModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<ItemAssignmentHistoryModel>>> GetAssignmentsForPerson(Guid id, PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var assignments = await itemAssignmentHistoryService.GetAssignmentsForPersonAsync(id, pagedQuery, cancellationToken);
        return Ok(assignments);
    }
}
