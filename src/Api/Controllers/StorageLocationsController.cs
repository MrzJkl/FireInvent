using FireInvent.Contract;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("storageLocations")]
public class StorageLocationsController(IStorageLocationService locationService, IItemService itemService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List all storage locations")]
    [EndpointDescription("Returns a list of all storage locations.")]
    [ProducesResponseType<List<StorageLocationModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StorageLocationModel>>> GetAll()
    {
        var locations = await locationService.GetAllStorageLocationsAsync();
        return Ok(locations);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get storage location by ID")]
    [EndpointDescription("Returns a storage location by its unique ID.")]
    [ProducesResponseType<StorageLocationModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StorageLocationModel>> GetById(Guid id)
    {
        var location = await locationService.GetStorageLocationByIdAsync(id);
        return location is null ? throw new NotFoundException() : (ActionResult<StorageLocationModel>)Ok(location);
    }

    [HttpPost]
    [EndpointSummary("Create a new storage location")]
    [EndpointDescription("Creates a new storage location.")]
    [ProducesResponseType<StorageLocationModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<ActionResult<StorageLocationModel>> Create(CreateOrUpdateStorageLocationModel model)
    {
        var created = await locationService.CreateStorageLocationAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update a storage location")]
    [EndpointDescription("Updates an existing storage location.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateStorageLocationModel model)
    {
        var success = await locationService.UpdateStorageLocationAsync(id, model);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete a storage location")]
    [EndpointDescription("Deletes a storage location by its unique ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await locationService.DeleteStorageLocationAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpGet("{id:guid}/items")]
    [EndpointSummary("List all items for a storage location")]
    [EndpointDescription("Returns all items assigned to a storage location.")]
    [ProducesResponseType<List<ItemModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ItemModel>>> GetItemsForLocation(Guid id)
    {
        var items = await itemService.GetItemsForStorageLocationAsync(id);
        return Ok(items);
    }
}
