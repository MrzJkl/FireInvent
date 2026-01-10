using FireInvent.Contract;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services.Keycloak;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

[ApiController]
[Route("/api-integrations")]
[Authorize(Roles = Roles.Admin)]
public class ApiIntegrationsController(IKeycloakApiIntegrationService keycloakAdminService) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Create API integration")]
    [EndpointDescription("Creates a new API integration with confidential client credentials. The credentials are shown only once.")]
    [ProducesResponseType<ApiIntegrationCredentialsModel>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiIntegrationCredentialsModel>> CreateApiIntegration(
        [FromBody] CreateApiIntegrationModel request,
        CancellationToken cancellationToken)
    {
        var credentials = await keycloakAdminService.CreateApiIntegrationAsync(
            request.Name,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetApiIntegrations),
            new { },
            credentials);
    }

    [HttpGet]
    [EndpointSummary("List API integrations")]
    [EndpointDescription("Returns a list of all API integrations. Client secrets are not included.")]
    [ProducesResponseType<List<ApiIntegrationModel>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ApiIntegrationModel>>> GetApiIntegrations(CancellationToken cancellationToken)
    {
        var integrations = await keycloakAdminService.GetApiIntegrationsAsync(cancellationToken);
        return Ok(integrations);
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Delete API integration")]
    [EndpointDescription("Deletes an API integration and revokes all access. This action cannot be undone.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteApiIntegration(Guid id, CancellationToken cancellationToken)
    {
        await keycloakAdminService.DeleteApiIntegrationAsync(id, cancellationToken);
        return NoContent();
    }
}
