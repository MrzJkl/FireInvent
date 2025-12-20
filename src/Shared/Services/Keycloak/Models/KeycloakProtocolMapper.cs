using System.Text.Json.Serialization;

namespace FireInvent.Shared.Services.Keycloak;

/// <summary>
/// Keycloak protocol mapper representation.
/// </summary>
internal class KeycloakProtocolMapper
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("protocol")]
    public string? Protocol { get; set; }

    [JsonPropertyName("protocolMapper")]
    public string? ProtocolMapperType { get; set; }

    [JsonPropertyName("config")]
    public Dictionary<string, string>? Config { get; set; }
}
