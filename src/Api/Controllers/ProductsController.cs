using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("products")]
public class ProductsController(IProductService productService, IVariantService variantService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List all products")]
    [EndpointDescription("Returns a list of all products.")]
    [ProducesResponseType<PagedResult<ProductModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductModel>>> GetAll(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var products = await productService.GetAllProductsAsync(pagedQuery, cancellationToken);
        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get product by ID")]
    [EndpointDescription("Returns a product by its unique ID.")]
    [ProducesResponseType<ProductModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductModel>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await productService.GetProductByIdAsync(id, cancellationToken);
        return product is null ? throw new NotFoundException() : (ActionResult<ProductModel>)Ok(product);
    }

    [HttpPost]
    [EndpointSummary("Create a new product")]
    [EndpointDescription("Creates a new product.")]
    [ProducesResponseType<ProductModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<ActionResult<ProductModel>> Create(CreateOrUpdateProductModel model, CancellationToken cancellationToken)
    {
        var created = await productService.CreateProductAsync(model, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update a product")]
    [EndpointDescription("Updates an existing product.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateProductModel model, CancellationToken cancellationToken)
    {
        var success = await productService.UpdateProductAsync(id, model, cancellationToken);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete a product")]
    [EndpointDescription("Deletes a product by its unique ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement + "," + Roles.Integration)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var success = await productService.DeleteProductAsync(id, cancellationToken);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpGet("{id:guid}/variants")]
    [EndpointSummary("List all variants for a product")]
    [EndpointDescription("Returns all variants for a specific product.")]
    [ProducesResponseType<PagedResult<VariantModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<VariantModel>>> GetVariantsForProduct(Guid id, PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var items = await variantService.GetVariantsForProductAsync(id, pagedQuery, cancellationToken);
        return Ok(items);
    }
}
