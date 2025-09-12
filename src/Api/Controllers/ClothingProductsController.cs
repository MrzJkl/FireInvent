using FireInvent.Contract;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("clothingProducts")]
public class ClothingProductsController(IClothingProductService productService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "List all clothing products", Description = "Returns a list of all clothing products.")]
    [SwaggerResponse(200, "List of clothing products", typeof(List<ProductModel>))]
    public async Task<ActionResult<List<ProductModel>>> GetAll()
    {
        var products = await productService.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get clothing product by ID", Description = "Returns a clothing product by its unique ID.")]
    [SwaggerResponse(200, "Clothing product found", typeof(ProductModel))]
    [SwaggerResponse(404, "Clothing product not found")]
    public async Task<ActionResult<ProductModel>> GetById(Guid id)
    {
        var product = await productService.GetProductByIdAsync(id);
        return product is null ? throw new NotFoundException() : (ActionResult<ProductModel>)Ok(product);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new clothing product", Description = "Creates a new clothing product.")]
    [SwaggerResponse(201, "Clothing product created", typeof(ProductModel))]
    [SwaggerResponse(409, "A product with the same name and manufacturer already exists")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<ActionResult<ProductModel>> Create(CreateProductModel model)
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
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Update(Guid id, ProductModel model)
    {
        if (id != model.Id)
            throw new IdMismatchException();

        try
        {
            var success = await productService.UpdateProductAsync(model);
            return success ? NoContent() : throw new NotFoundException();
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
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await productService.DeleteProductAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }
}
