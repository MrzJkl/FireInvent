using FireInvent.Contract;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("clothingVariants")]
public class ClothingVariantsController(IClothingVariantService variantService, IClothingItemService itemService) : ControllerBase
{

    [HttpGet]
    [SwaggerOperation(Summary = "List all clothing variants", Description = "Returns a list of all clothing variants.")]
    [SwaggerResponse(200, "List of clothing variants", typeof(List<VariantModel>))]
    public async Task<ActionResult<List<VariantModel>>> GetAll()
    {
        var variants = await variantService.GetAllVariantsAsync();
        return Ok(variants);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get clothing variant by ID", Description = "Returns a clothing variant by its unique ID.")]
    [SwaggerResponse(200, "Clothing variant found", typeof(VariantModel))]
    [SwaggerResponse(404, "Clothing variant not found")]
    public async Task<ActionResult<VariantModel>> GetById(Guid id)
    {
        var variant = await variantService.GetVariantByIdAsync(id);
        return variant is null ? throw new NotFoundException() : (ActionResult<VariantModel>)Ok(variant);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new clothing variant", Description = "Creates a new clothing variant.")]
    [SwaggerResponse(201, "Clothing variant created", typeof(VariantModel))]
    [SwaggerResponse(409, "A clothing variant with the same name already exists for this product")]
    [SwaggerResponse(400, "Referenced product does not exist")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<ActionResult<VariantModel>> Create(CreateVariantModel model)
    {
        var created = await variantService.CreateVariantAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Update a clothing variant", Description = "Updates an existing clothing variant.")]
    [SwaggerResponse(204, "Clothing variant updated")]
    [SwaggerResponse(400, "ID mismatch")]
    [SwaggerResponse(404, "Clothing variant not found")]
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
    [SwaggerOperation(Summary = "Delete a clothing variant", Description = "Deletes a clothing variant by its unique ID.")]
    [SwaggerResponse(204, "Clothing variant deleted")]
    [SwaggerResponse(404, "Clothing variant not found")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await variantService.DeleteVariantAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpGet("{id:guid}/items")]
    [SwaggerOperation(Summary = "List all clothing items for a variant", Description = "Returns all clothing items for a specific variant.")]
    [SwaggerResponse(200, "List of clothing items", typeof(List<ItemModel>))]
    [SwaggerResponse(404, "Clothing variant not found")]
    public async Task<ActionResult<List<ItemModel>>> GetItemsForVariant(Guid id)
    {
        var items = await itemService.GetItemsForVariantAsync(id);
        return Ok(items);
    }
}
