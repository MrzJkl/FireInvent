using FireInvent.Contract;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("storageLocations")]
public class StorageLocationsController(IStorageLocationService locationService, IItemService itemService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "List all storage locations", Description = "Returns a list of all storage locations.")]
    [SwaggerResponse(200, "List of storage locations", typeof(List<StorageLocationModel>))]
    public async Task<ActionResult<List<StorageLocationModel>>> GetAll()
    {
        var locations = await locationService.GetAllStorageLocationsAsync();
        return Ok(locations);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get storage location by ID", Description = "Returns a storage location by its unique ID.")]
    [SwaggerResponse(200, "Storage location found", typeof(StorageLocationModel))]
    [SwaggerResponse(404, "Storage location not found")]
    public async Task<ActionResult<StorageLocationModel>> GetById(Guid id)
    {
        var location = await locationService.GetStorageLocationByIdAsync(id);
        return location is null ? throw new NotFoundException() : (ActionResult<StorageLocationModel>)Ok(location);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new storage location", Description = "Creates a new storage location.")]
    [SwaggerResponse(201, "Storage location created", typeof(StorageLocationModel))]
    [SwaggerResponse(409, "A storage location with the same name already exists")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<ActionResult<StorageLocationModel>> Create(CreateOrUpdateStorageLocationModel model)
    {
        var created = await locationService.CreateStorageLocationAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Update a storage location", Description = "Updates an existing storage location.")]
    [SwaggerResponse(204, "Storage location updated")]
    [SwaggerResponse(400, "ID mismatch")]
    [SwaggerResponse(404, "Storage location not found")]
    [SwaggerResponse(409, "A storage location with the same name already exists")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateStorageLocationModel model)
    {
        if (id != model.Id)
            throw new IdMismatchException();

        var success = await locationService.UpdateStorageLocationAsync(model);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Delete a storage location", Description = "Deletes a storage location by its unique ID.")]
    [SwaggerResponse(204, "Storage location deleted")]
    [SwaggerResponse(404, "Storage location not found")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await locationService.DeleteStorageLocationAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpGet("{id:guid}/items")]
    [SwaggerOperation(Summary = "List all items for a storage location", Description = "Returns all items assigned to a storage location.")]
    [SwaggerResponse(200, "List of items", typeof(List<ItemModel>))]
    [SwaggerResponse(404, "Storage location not found")]
    public async Task<ActionResult<List<ItemModel>>> GetItemsForLocation(Guid id)
    {
        var items = await itemService.GetItemsForStorageLocationAsync(id);
        return Ok(items);
    }
}
