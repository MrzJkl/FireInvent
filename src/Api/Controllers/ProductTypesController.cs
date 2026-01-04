using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("product-types")]
public class ProductTypesController(IProductTypeService productTypeService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List all productTypes")]
    [EndpointDescription("Returns a list of all productTypes.")]
    [ProducesResponseType<PagedResult<ProductTypeModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductTypeModel>>> GetAll(PagedQuery pagedQuery, CancellationToken cancellationToken)
    {
        var productTypes = await productTypeService.GetAllProductTypesAsync(pagedQuery, cancellationToken);
        return Ok(productTypes);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get productType by ID")]
    [EndpointDescription("Returns a productType by its unique ID.")]
    [ProducesResponseType<ProductTypeModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductTypeModel>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var productType = await productTypeService.GetProductTypeByIdAsync(id, cancellationToken);
        return productType is null ? throw new NotFoundException() : (ActionResult<ProductTypeModel>)Ok(productType);
    }

    [HttpPost]
    [EndpointSummary("Create a new productType")]
    [EndpointDescription("Creates a new productType.")]
    [ProducesResponseType<ProductTypeModel>(StatusCodes.Status201Created)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Integration)]
    public async Task<ActionResult<ProductTypeModel>> Create(CreateOrUpdateProductTypeModel model, CancellationToken cancellationToken)
    {
        var created = await productTypeService.CreateProductTypeAsync(model, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update a productType")]
    [EndpointDescription("Updates an existing productType.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Integration)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateProductTypeModel model, CancellationToken cancellationToken)
    {
        var success = await productTypeService.UpdateProductTypeAsync(id, model, cancellationToken);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete a productType")]
    [EndpointDescription("Deletes a productType by its unique ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Integration)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var success = await productTypeService.DeleteProductTypeAsync(id, cancellationToken);
        return success ? NoContent() : throw new NotFoundException();
    }
}
