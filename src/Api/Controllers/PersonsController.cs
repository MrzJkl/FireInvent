using FireInvent.Contract;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("persons")]
public class PersonsController(IPersonService personService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "List all persons", Description = "Returns a list of all persons.")]
    [SwaggerResponse(200, "List of persons", typeof(List<PersonModel>))]
    public async Task<ActionResult<List<PersonModel>>> GetAll()
    {
        var persons = await personService.GetAllPersonsAsync();
        return Ok(persons);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get person by ID", Description = "Returns a single person by their unique ID.")]
    [SwaggerResponse(200, "Person found", typeof(PersonModel))]
    [SwaggerResponse(404, "Person not found")]
    public async Task<ActionResult<PersonModel>> GetById(Guid id)
    {
        var person = await personService.GetPersonByIdAsync(id);
        return person is null ? throw new NotFoundException() : (ActionResult<PersonModel>)Ok(person);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new person", Description = "Creates a new person entry.")]
    [SwaggerResponse(201, "Person created", typeof(PersonModel))]
    [SwaggerResponse(409, "Person with same name or external ID already exists")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<ActionResult<PersonModel>> Create(CreatePersonModel model)
    {
        var created = await personService.CreatePersonAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Update a person", Description = "Updates an existing person.")]
    [SwaggerResponse(204, "Person updated")]
    [SwaggerResponse(404, "Person not found")]
    [SwaggerResponse(400, "ID mismatch")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Update(Guid id, PersonModel model)
    {
        if (id != model.Id)
            throw new IdMismatchException();

        var success = await personService.UpdatePersonAsync(model);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Delete a person", Description = "Deletes a person by ID.")]
    [SwaggerResponse(204, "Person deleted")]
    [SwaggerResponse(404, "Person not found")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await personService.DeletePersonAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }
}
