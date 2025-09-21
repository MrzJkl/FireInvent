using FireInvent.Contract;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("variants")]
public class VariantsController(IVariantService variantService, IItemService itemService) : ControllerBase
{

    [HttpGet]
    [SwaggerOperation(Summary = "List all variants", Description = "Returns a list of all variants.")]
    [SwaggerResponse(200, "List of variants", typeof(List<VariantModel>))]
    public async Task<ActionResult<List<VariantModel>>> GetAll()
    {
        var variants = await variantService.GetAllVariantsAsync();
        return Ok(variants);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get variant by ID", Description = "Returns a variant by its unique ID.")]
    [SwaggerResponse(200, "variant found", typeof(VariantModel))]
    [SwaggerResponse(404, "variant not found")]
    public async Task<ActionResult<VariantModel>> GetById(Guid id)
    {
        var variant = await variantService.GetVariantByIdAsync(id);
        return variant is null ? throw new NotFoundException() : (ActionResult<VariantModel>)Ok(variant);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new variant", Description = "Creates a new variant.")]
    [SwaggerResponse(201, "variant created", typeof(VariantModel))]
    [SwaggerResponse(409, "A variant with the same name already exists for this product")]
    [SwaggerResponse(400, "Referenced product does not exist")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<ActionResult<VariantModel>> Create(CreateVariantModel model)
    {
        var created = await variantService.CreateVariantAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Update a variant", Description = "Updates an existing variant.")]
    [SwaggerResponse(204, "variant updated")]
    [SwaggerResponse(400, "ID mismatch")]
    [SwaggerResponse(404, "variant not found")]
    [SwaggerResponse(409, "Another variant with the same name already exists for this product")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Update(Guid id, VariantModel model)
    {
        if (id != model.Id)
            throw new IdMismatchException();

        var success = await variantService.UpdateVariantAsync(model);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Delete a variant", Description = "Deletes a variant by its unique ID.")]
    [SwaggerResponse(204, "variant deleted")]
    [SwaggerResponse(404, "variant not found")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await variantService.DeleteVariantAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpGet("{id:guid}/items")]
    [SwaggerOperation(Summary = "List all items for a variant", Description = "Returns all items for a specific variant.")]
    [SwaggerResponse(200, "List of items", typeof(List<ItemModel>))]
    [SwaggerResponse(404, "variant not found")]
    public async Task<ActionResult<List<ItemModel>>> GetItemsForVariant(Guid id)
    {
        var items = await itemService.GetItemsForVariantAsync(id);
        return Ok(items);
    }
}
