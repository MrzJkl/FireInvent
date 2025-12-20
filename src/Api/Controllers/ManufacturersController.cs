using FireInvent.Contract;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("manufacturers")]
public class ManufacturersController(IManufacturerService manufacturerService, IProductService productService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List all manufacturers")]
    [EndpointDescription("Returns a list of all manufacturers.")]
    [ProducesResponseType<List<ManufacturerModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ManufacturerModel>>> GetAll()
    {
        var manufacturers = await manufacturerService.GetAllManufacturersAsync();
        return Ok(manufacturers);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get manufacturer by ID")]
    [EndpointDescription("Returns a manufacturer by its unique ID.")]
    [ProducesResponseType<ManufacturerModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ManufacturerModel>> GetById(Guid id)
    {
        var manufacturer = await manufacturerService.GetManufacturerByIdAsync(id);
        return manufacturer is null ? throw new NotFoundException() : (ActionResult<ManufacturerModel>)Ok(manufacturer);
    }

    [HttpPost]
    [EndpointSummary("Create a new manufacturer")]
    [EndpointDescription("Creates a new manufacturer.")]
    [ProducesResponseType<ManufacturerModel>(StatusCodes.Status201Created)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<ActionResult<ManufacturerModel>> Create(CreateOrUpdateManufacturerModel model)
    {
        var created = await manufacturerService.CreateManufacturerAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update a manufacturer")]
    [EndpointDescription("Updates an existing manufacturer.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateManufacturerModel model)
    {
        var success = await manufacturerService.UpdateManufacturerAsync(id, model);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete a manufacturer")]
    [EndpointDescription("Deletes a manufacturer by its unique ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = Roles.Admin + "," + Roles.Procurement)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await manufacturerService.DeleteManufacturerAsync(id);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpGet("{id:guid}/products")]
    [EndpointSummary("List all products for a manufacturer")]
    [EndpointDescription("Returns all products for a specific manufacturer.")]
    [ProducesResponseType<List<ProductModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ProductModel>>> GetProductsForManufacturer(Guid id)
    {
        var items = await productService.GetProductsForManufacturer(id);
        return Ok(items);
    }
}
