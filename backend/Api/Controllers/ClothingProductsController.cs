using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("clothingProducts")]
public class ClothingProductsController(ClothingProductService productService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "List all clothing products", Description = "Returns a list of all clothing products.")]
    [SwaggerResponse(200, "List of clothing products", typeof(List<ClothingProductModel>))]
    public async Task<ActionResult<List<ClothingProductModel>>> GetAll()
    {
        var products = await productService.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get clothing product by ID", Description = "Returns a clothing product by its unique ID.")]
    [SwaggerResponse(200, "Clothing product found", typeof(ClothingProductModel))]
    [SwaggerResponse(404, "Clothing product not found")]
    public async Task<ActionResult<ClothingProductModel>> GetById(Guid id)
    {
        var product = await productService.GetProductByIdAsync(id);
        if (product is null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new clothing product", Description = "Creates a new clothing product.")]
    [SwaggerResponse(201, "Clothing product created", typeof(ClothingProductModel))]
    [SwaggerResponse(409, "A product with the same name and manufacturer already exists")]
    public async Task<ActionResult<ClothingProductModel>> Create(CreateClothingProductModel model)
    {
        var created = await productService.CreateProductAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Update a clothing product", Description = "Updates an existing clothing product.")]
    [SwaggerResponse(204, "Clothing product updated")]
    [SwaggerResponse(400, "ID mismatch")]
    [SwaggerResponse(404, "Clothing product not found")]
    [SwaggerResponse(409, "A product with the same name and manufacturer already exists")]
    public async Task<IActionResult> Update(Guid id, ClothingProductModel model)
    {
        if (id != model.Id)
            throw new IdMismatchException();

        try
        {
            var success = await productService.UpdateProductAsync(model);
            return success ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Delete a clothing product", Description = "Deletes a clothing product by its unique ID.")]
    [SwaggerResponse(204, "Clothing product deleted")]
    [SwaggerResponse(404, "Clothing product not found")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await productService.DeleteProductAsync(id);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}/variants")]
    [SwaggerOperation(Summary = "Delete a clothing product", Description = "Deletes a clothing product by its unique ID.")]
    [SwaggerResponse(204, "Clothing product deleted")]
    [SwaggerResponse(404, "Clothing product not found")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await productService.DeleteProductAsync(id);
        return success ? NoContent() : NotFound();
    }
}
