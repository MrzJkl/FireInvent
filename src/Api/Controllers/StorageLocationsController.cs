﻿using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("storage-locations")]
public class StorageLocationsController(IStorageLocationService locationService, IItemAssignmentHistoryService itemAssignmentHistoryService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List all storage locations")]
    [EndpointDescription("Returns a list of all storage locations.")]
    [ProducesResponseType<PagedResult<StorageLocationModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<StorageLocationModel>>> GetAll(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var storageLocations = await locationService.GetAllStorageLocationsAsync(pagedQuery, cancellationToken);
        return Ok(storageLocations);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get storage location by ID")]
    [EndpointDescription("Returns a storage location by its unique ID.")]
    [ProducesResponseType<StorageLocationModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StorageLocationModel>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var location = await locationService.GetStorageLocationByIdAsync(id, cancellationToken);
        return location is null ? throw new NotFoundException() : (ActionResult<StorageLocationModel>)Ok(location);
    }

    [HttpPost]
    [EndpointSummary("Create a new storage location")]
    [EndpointDescription("Creates a new storage location.")]
    [ProducesResponseType<StorageLocationModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<ActionResult<StorageLocationModel>> Create(CreateOrUpdateStorageLocationModel model, CancellationToken cancellationToken)
    {
        var created = await locationService.CreateStorageLocationAsync(model, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update a storage location")]
    [EndpointDescription("Updates an existing storage location.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateStorageLocationModel model, CancellationToken cancellationToken)
    {
        var success = await locationService.UpdateStorageLocationAsync(id, model, cancellationToken);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete a storage location")]
    [EndpointDescription(
        "Deletes a storage location by its unique ID. " +
        "DELETION RESTRICTED: This operation will fail if there are any items currently stored in this location. " +
        "Please reassign or remove all items from this storage location first.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var success = await locationService.DeleteStorageLocationAsync(id, cancellationToken);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpGet("{id:guid}/assignments")]
    [EndpointSummary("List all assignments for a storage location")]
    [EndpointDescription("Returns all assignments for a specific storage location.")]
    [ProducesResponseType<PagedResult<ItemAssignmentHistoryModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<ItemAssignmentHistoryModel>>> GetAssignmentsForLocation(Guid id, PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var assignments = await itemAssignmentHistoryService.GetAssignmentsForStorageLocationAsync(id, pagedQuery, cancellationToken);
        return Ok(assignments);
    }
}
