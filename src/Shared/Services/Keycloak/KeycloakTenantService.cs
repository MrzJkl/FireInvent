using FireInvent.Contract.Extensions;
using FireInvent.Contract.Exceptions;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace FireInvent.Shared.Services.Keycloak;

/// <summary>
/// Service for managing Keycloak Organizations for tenant provisioning.
/// All users live in a single realm; tenants are represented by Organizations.
/// </summary>
public class KeycloakTenantService(
    KeycloakHttpClient keycloakClient,
    ILogger<KeycloakTenantService> logger) : IKeycloakTenantService
{

    /// <summary>
    /// Creates a tenant Organization with a name and description. Returns the GUID of the created organization.
    /// </summary>
    public async Task<Guid> CreateTenantOrganizationAsync(string name, string? description, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Organization name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        var alias = SanitizeNameForKeycloak(name);

        try
        {
            var listResponse = await keycloakClient.GetAsync($"admin/realms/{Uri.EscapeDataString(keycloakClient.Realm)}/organizations");
            if (!listResponse.IsSuccessStatusCode)
            {
                var error = await listResponse.Content.ReadAsStringAsync();
                logger.LogError("Failed to list organizations: {StatusCode} - {Error}", listResponse.StatusCode, error);
                throw new KeycloakException();
            }

            var existingOrgs = await listResponse.Content.ReadFromJsonAsync<List<JsonElement>>(keycloakClient.JsonOptions) ?? new List<JsonElement>();
            var organizationExists = existingOrgs.Any(org =>
            {
                if (org.TryGetProperty("name", out var orgNameProp))
                {
                    var orgName = orgNameProp.GetString();
                    return orgName == name;
                }

                if (org.TryGetProperty("alias", out var orgAliasProp))
                {
                    var orgAlias = orgAliasProp.GetString();
                    return orgAlias == alias;
                }

                return false;
            });
            if (organizationExists)
            {
                logger.LogWarning("Organization with name '{Name}' already exists.", name.SanitizeForLogging());
                throw new KeycloakException();
            }

            var payload = new
            {
                name,
                alias,
                description,
            };

            var createResponse = await keycloakClient.PostAsJsonAsync(
                $"admin/realms/{Uri.EscapeDataString(keycloakClient.Realm)}/organizations",
                payload);

            if (createResponse.IsSuccessStatusCode)
            {
                logger.LogInformation("Created organization '{Name}'", name.SanitizeForLogging());

                Uri? location = createResponse.Headers.Location;
                if (location == null)
                {
                    if (createResponse.Headers.TryGetValues("Location", out var values))
                    {
                        var first = values.FirstOrDefault();
                        if (!string.IsNullOrEmpty(first))
                        {
                            location = Uri.TryCreate(first, UriKind.RelativeOrAbsolute, out var tmp) ? tmp : null;
                        }
                    }
                }

                if (location == null)
                {
                    logger.LogError("Create organization response missing Location header.");
                    throw new KeycloakException();
                }

                // Keycloak is expected to return a Location header pointing to the created organization resource,
                // e.g. ".../organizations/{id}". The organization ID is taken from the last path segment.
                var lastSegment = location.Segments.LastOrDefault()?.Trim('/');
                if (!string.IsNullOrWhiteSpace(lastSegment) && Guid.TryParse(lastSegment, out var idFromLocation))
                {
                    return idFromLocation;
                }

                logger.LogError("Created organization resource did not contain a valid 'id'.");
                throw new KeycloakException();
            }

            var createError = await createResponse.Content.ReadAsStringAsync();
            logger.LogError("Failed to create organization '{Name}': {StatusCode} - {Error}", name.SanitizeForLogging(), createResponse.StatusCode, createError);
            throw new KeycloakException();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating organization {Name}", name.SanitizeForLogging());
            throw new KeycloakException();
        }
    }

    public async Task UpdateTenantOrganizationNameAsync(Guid organizationId, string newName, string? newDescription, CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
            throw new ArgumentException("Organization ID cannot be empty.", nameof(organizationId));
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("New name cannot be empty.", nameof(newName));

        var alias = SanitizeNameForKeycloak(newName);

        try
        {
            var payload = new
            {
                name = newName,
                description = newDescription,
                alias
            };

            var response = await keycloakClient.PutAsJsonAsync(
                $"admin/realms/{Uri.EscapeDataString(keycloakClient.Realm)}/organizations/{organizationId}",
                payload);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                logger.LogError("Failed to update organization name '{Name}' ({Id}): {StatusCode} - {Error}", newName.SanitizeForLogging(), organizationId, response.StatusCode, error);
                throw new KeycloakException();
            }

            logger.LogInformation("Updated organization ({Id}) name to '{Name}'", organizationId, newName.SanitizeForLogging());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating organization name ({Id})", organizationId);
            throw new KeycloakException();
        }
    }

    private static string SanitizeNameForKeycloak(string name)
    {
        var sanitized = new string(name
            .Trim()
            .ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray());

        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, "-+", "-");

        return sanitized.Trim('-');
    }
}
