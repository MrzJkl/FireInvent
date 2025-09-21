using FireInvent.Contract;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("productTypes")]
public class ProductTypesController(IProductTypeService productTypeService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "List all productTypes", Description = "Returns a list of all productTypes.")]
    [SwaggerResponse(200, "List of productTypes", typeof(List<ProductTypeModel>))]
    public async Task<ActionResult<List<ProductTypeModel>>> GetAll()
    {
        var productTypes = await productTypeService.GetAllProductTypesAsync();
        return Ok(productTypes);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get productType by ID", Description = "Returns a productType by its unique ID.")]
    [SwaggerResponse(200, "ProductType found", typeof(ProductTypeModel))]
    [SwaggerResponse(404, "ProductType not found")]
    public async Task<ActionResult<ProductTypeModel>> GetById(Guid id)
    {
        var productType = await productTypeService.GetProductTypeByIdAsync(id);
        return productType is null ? throw new NotFoundException() : (ActionResult<ProductTypeModel>)Ok(productType);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new productType", Description = "Creates a new productType.")]
    [SwaggerResponse(201, "ProductType created", typeof(ProductTypeModel))]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ProductTypeModel>> Create(CreateProductTypeModel model)
    {
        var created = await productTypeService.CreateProductTypeAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Update a productType", Description = "Updates an existing productType.")]
    [SwaggerResponse(204, "ProductType updated")]
    [SwaggerResponse(400, "ID mismatch")]
    [SwaggerResponse(404, "ProductType not found")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(Guid id, ProductTypeModel model)
    {
        if (id != model.Id)
            throw new IdMismatchException();

        var success = await productTypeService.UpdateProductTypeAsync(model);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Delete a productType", Description = "Deletes a productType by its unique ID.")]
    [SwaggerResponse(204, "ProductType deleted")]
    [SwaggerResponse(404, "ProductType not found")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await productTypeService.DeleteProductTypeAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }
}
