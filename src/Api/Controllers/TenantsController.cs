using FireInvent.Contract;
using FireInvent.Contract.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("tenants")]
[Authorize(Roles = Roles.SystemAdmin)]
public class TenantsController(ITenantService tenantService) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List all tenants (System Admin)")]
    [EndpointDescription("Returns a list of all tenants in the system. Only accessible to system administrators.")]
    [ProducesResponseType<List<TenantModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TenantModel>>> GetAll(CancellationToken cancellationToken)
    {
        var tenants = await tenantService.GetAllTenantsAsync(cancellationToken);
        return Ok(tenants);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get tenant by ID (System Admin)")]
    [EndpointDescription("Returns a tenant by its unique ID. Only accessible to system administrators.")]
    [ProducesResponseType<TenantModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantModel>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await tenantService.GetTenantByIdAsync(id, cancellationToken);
        return tenant is null ? throw new NotFoundException() : (ActionResult<TenantModel>)Ok(tenant);
    }

    [HttpPost]
    [EndpointSummary("Create a new tenant (System Admin)")]
    [EndpointDescription("Creates a new tenant with a unique realm and name. Only accessible to system administrators.")]
    [ProducesResponseType<TenantModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TenantModel>> Create(CreateOrUpdateTenantModel model, CancellationToken cancellationToken)
    {
        var created = await tenantService.CreateTenantAsync(model, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Update a tenant (System Admin)")]
    [EndpointDescription("Updates an existing tenant. Only accessible to system administrators.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, CreateOrUpdateTenantModel model, CancellationToken cancellationToken)
    {
        var success = await tenantService.UpdateTenantAsync(id, model, cancellationToken);
        return success ? NoContent() : throw new NotFoundException();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete a tenant (System Admin)")]
    [EndpointDescription("Deletes a tenant by its unique ID. Only accessible to system administrators.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var success = await tenantService.DeleteTenantAsync(id, cancellationToken);
        return success ? NoContent() : throw new NotFoundException();
    }
}
