using System.Text.Json.Serialization;

namespace FireInvent.Shared.Services.Keycloak;

/// <summary>
/// Keycloak client secret response.
/// </summary>
internal class KeycloakClientSecretResponse
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
