using FlameGuardLaundry.Shared.Exceptions;
using FlameGuardLaundry.Shared.Models;
using FlameGuardLaundry.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FlameGuardLaundry.Api.Controllers;

[ApiController]
[Route("clothingVariants")]
public class ClothingVariantsController(ClothingVariantService variantService, ClothingItemService itemService) : ControllerBase
{

    [HttpGet]
    [SwaggerOperation(Summary = "List all clothing variants", Description = "Returns a list of all clothing variants.")]
    [SwaggerResponse(200, "List of clothing variants", typeof(List<ClothingVariantModel>))]
    public async Task<ActionResult<List<ClothingVariantModel>>> GetAll()
    {
        var variants = await variantService.GetAllVariantsAsync();
        return Ok(variants);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get clothing variant by ID", Description = "Returns a clothing variant by its unique ID.")]
    [SwaggerResponse(200, "Clothing variant found", typeof(ClothingVariantModel))]
    [SwaggerResponse(404, "Clothing variant not found")]
    public async Task<ActionResult<ClothingVariantModel>> GetById(Guid id)
    {
        var variant = await variantService.GetVariantByIdAsync(id);
        if (variant is null)
            return NotFound();

        return Ok(variant);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new clothing variant", Description = "Creates a new clothing variant.")]
    [SwaggerResponse(201, "Clothing variant created", typeof(ClothingVariantModel))]
    [SwaggerResponse(409, "A clothing variant with the same name already exists for this product")]
    [SwaggerResponse(400, "Referenced product does not exist")]
    public async Task<ActionResult<ClothingVariantModel>> Create(CreateClothingVariantModel model)
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
    public async Task<IActionResult> Update(Guid id, ClothingVariantModel model)
    {
        if (id != model.Id)
            throw new IdMismatchException();

        var success = await variantService.UpdateVariantAsync(model);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Delete a clothing variant", Description = "Deletes a clothing variant by its unique ID.")]
    [SwaggerResponse(204, "Clothing variant deleted")]
    [SwaggerResponse(404, "Clothing variant not found")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await variantService.DeleteVariantAsync(id);
        return success ? NoContent() : NotFound();
    }

    [HttpGet("{variantId:guid}/items")]
    [SwaggerOperation(Summary = "List all clothing items for a variant", Description = "Returns all clothing items for a specific variant.")]
    [SwaggerResponse(200, "List of clothing items", typeof(List<ClothingItemModel>))]
    [SwaggerResponse(404, "Clothing variant not found")]
    public async Task<ActionResult<List<ClothingItemModel>>> GetItemsForVariant(Guid variantId, [FromServices] ClothingItemService itemService)
    {
        var items = await itemService.GetItemsForVariantAsync(variantId);
        return Ok(items);
    }
}
