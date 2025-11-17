using FireInvent.Contract;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("products")]
public class ProductsController(IProductService productService, IVariantService variantService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "List all products", Description = "Returns a list of all products.")]
    [SwaggerResponse(200, "List of products", typeof(List<ProductModel>))]
    public async Task<ActionResult<List<ProductModel>>> GetAll()
    {
        var products = await productService.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get product by ID", Description = "Returns a product by its unique ID.")]
    [SwaggerResponse(200, "Product found", typeof(ProductModel))]
    [SwaggerResponse(404, "Product not found")]
    public async Task<ActionResult<ProductModel>> GetById(Guid id)
    {
        var product = await productService.GetProductByIdAsync(id);
        return product is null ? throw new NotFoundException() : (ActionResult<ProductModel>)Ok(product);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new product", Description = "Creates a new product.")]
    [SwaggerResponse(201, "Product created", typeof(ProductModel))]
    [SwaggerResponse(409, "A product with the same name and manufacturer already exists")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<ActionResult<ProductModel>> Create(CreateOrUpdateProductModel model)
    {
        var created = await productService.CreateProductAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Update a product", Description = "Updates an existing product.")]
    [SwaggerResponse(204, "Product updated")]
    [SwaggerResponse(400, "ID mismatch")]
    [SwaggerResponse(404, "Product not found")]
    [SwaggerResponse(409, "A product with the same name and manufacturer already exists")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateProductModel model)
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
    [SwaggerOperation(Summary = "Delete a product", Description = "Deletes a product by its unique ID.")]
    [SwaggerResponse(204, "Product deleted")]
    [SwaggerResponse(404, "Product not found")]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await productService.DeleteProductAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpGet("{id:guid}/variants")]
    [SwaggerOperation(Summary = "List all variants for a product", Description = "Returns all variants for a specific product.")]
    [SwaggerResponse(200, "List of variants", typeof(List<VariantModel>))]
    [SwaggerResponse(404, "Product not found")]
    public async Task<ActionResult<List<ItemModel>>> GetVariantsForProduct(Guid id)
    {
        var items = await variantService.GetVariantsForProductAsync(id);
        return Ok(items);
    }
}
