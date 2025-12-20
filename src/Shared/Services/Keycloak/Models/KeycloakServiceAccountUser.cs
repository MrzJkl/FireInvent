using System.Text.Json.Serialization;

namespace FireInvent.Shared.Services.Keycloak;

/// <summary>
/// Keycloak service account user representation.
/// </summary>
internal class KeycloakServiceAccountUser
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }
}
