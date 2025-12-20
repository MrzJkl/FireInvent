using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("variants")]
public class VariantsController(IVariantService variantService, IItemService itemService) : ControllerBase
{

    [HttpGet]
    [EndpointSummary("List all variants")]
    [EndpointDescription("Returns a list of all variants.")]
    [ProducesResponseType<List<VariantModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VariantModel>>> GetAll()
    {
        var variants = await variantService.GetAllVariantsAsync();
        return Ok(variants);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get variant by ID")]
    [EndpointDescription("Returns a variant by its unique ID.")]
    [ProducesResponseType<VariantModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VariantModel>> GetById(Guid id)
    {
        var variant = await variantService.GetVariantByIdAsync(id);
        return variant is null ? throw new NotFoundException() : (ActionResult<VariantModel>)Ok(variant);
    }

    [HttpPost]
    [EndpointSummary("Create a new variant")]
    [EndpointDescription("Creates a new variant.")]
    [ProducesResponseType<VariantModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<ActionResult<VariantModel>> Create(CreateOrUpdateVariantModel model)
    {
        var created = await variantService.CreateVariantAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update a variant")]
    [EndpointDescription("Updates an existing variant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateVariantModel model)
    {
        var success = await variantService.UpdateVariantAsync(id, model);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete a variant")]
    [EndpointDescription("Deletes a variant by its unique ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await variantService.DeleteVariantAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpGet("{id:guid}/items")]
    [EndpointSummary("List all items for a variant")]
    [EndpointDescription("Returns all items for a specific variant.")]
    [ProducesResponseType<List<ItemModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ItemModel>>> GetItemsForVariant(Guid id)
    {
        var items = await itemService.GetItemsForVariantAsync(id);
        return Ok(items);
    }
}
