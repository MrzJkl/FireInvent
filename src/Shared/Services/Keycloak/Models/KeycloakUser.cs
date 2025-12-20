using System.Text.Json.Serialization;

namespace FireInvent.Shared.Services.Keycloak;

/// <summary>
/// Keycloak user representation.
/// </summary>
internal class KeycloakUser
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("emailVerified")]
    public bool? EmailVerified { get; set; }

    [JsonPropertyName("createdTimestamp")]
    public long? CreatedTimestamp { get; set; }
}
