using FireInvent.Contract;
using FireInvent.Shared.Exceptions;
using FireInvent.Shared.Models;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FireInvent.Api.Controllers;

/// <summary>
/// Controller for managing API integrations (third-party API access).
/// Only administrators can manage API integrations.
/// </summary>
[ApiController]
[Route("/api-integrations")]
[Authorize(Roles = Roles.Admin)]
public class ApiIntegrationsController(IKeycloakAdminService keycloakAdminService) : ControllerBase
{
    /// <summary>
    /// Creates a new API integration with confidential client credentials.
    /// The credentials (client ID and secret) are returned only once and should be stored securely by the client.
    /// </summary>
    /// <param name="request">The API integration creation request.</param>
    /// <returns>The credentials for the newly created integration.</returns>
    [HttpPost]
    [EndpointSummary("Create API integration")]
    [EndpointDescription("Creates a new API integration with confidential client credentials. The credentials are shown only once.")]
    [ProducesResponseType<ApiIntegrationCredentials>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiIntegrationCredentials>> CreateApiIntegration(
        [FromBody] CreateApiIntegrationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "Name is required." });
        }

        try
        {
            var credentials = await keycloakAdminService.CreateApiIntegrationAsync(
                request.Name,
                request.Description);

            return CreatedAtAction(
                nameof(GetApiIntegrations),
                new { },
                credentials);
        }
        catch (ConflictException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to create API integration.", details = ex.Message });
        }
    }

    /// <summary>
    /// Lists all existing API integrations.
    /// </summary>
    /// <returns>A list of API integrations.</returns>
    [HttpGet]
    [EndpointSummary("List API integrations")]
    [EndpointDescription("Returns a list of all API integrations. Client secrets are not included.")]
    [ProducesResponseType<List<ApiIntegrationListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ApiIntegrationListItem>>> GetApiIntegrations()
    {
        try
        {
            var integrations = await keycloakAdminService.GetApiIntegrationsAsync();
            return Ok(integrations);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve API integrations.", details = ex.Message });
        }
    }

    /// <summary>
    /// Deletes an API integration by its client ID.
    /// This revokes all access for the integration.
    /// </summary>
    /// <param name="clientId">The client ID of the integration to delete.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{clientId}")]
    [EndpointSummary("Delete API integration")]
    [EndpointDescription("Deletes an API integration and revokes all access. This action cannot be undone.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteApiIntegration(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return BadRequest(new { error = "Client ID is required." });
        }

        try
        {
            await keycloakAdminService.DeleteApiIntegrationAsync(clientId);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to delete API integration.", details = ex.Message });
        }
    }
}
