using FireInvent.Contract;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("persons")]
public class PersonsController(IPersonService personService, IItemService itemService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List all persons")]
    [EndpointDescription("Returns a list of all persons.")]
    [ProducesResponseType<List<PersonModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PersonModel>>> GetAll()
    {
        var persons = await personService.GetAllPersonsAsync();
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
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
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
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
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
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await personService.DeletePersonAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpGet("{id:guid}/items")]
    [EndpointSummary("List all items assigned to a person")]
    [EndpointDescription("Returns all items assigned to a specific person.")]
    [ProducesResponseType<List<ItemModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ItemModel>>> GetVariantsForProduct(Guid id)
    {
        var items = await itemService.GetItemsAssignedToPersonAsync(id);
        return Ok(items);
    }
}
