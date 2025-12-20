using System.Text.Json.Serialization;

namespace FireInvent.Shared.Services.Keycloak;

/// <summary>
/// Keycloak client representation.
/// </summary>
internal class KeycloakClient
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("clientId")]
    public string? ClientId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("clientAuthenticatorType")]
    public string? ClientAuthenticatorType { get; set; }

    [JsonPropertyName("publicClient")]
    public bool? PublicClient { get; set; }

    [JsonPropertyName("serviceAccountsEnabled")]
    public bool? ServiceAccountsEnabled { get; set; }

    [JsonPropertyName("standardFlowEnabled")]
    public bool? StandardFlowEnabled { get; set; }

    [JsonPropertyName("implicitFlowEnabled")]
    public bool? ImplicitFlowEnabled { get; set; }

    [JsonPropertyName("directAccessGrantsEnabled")]
    public bool? DirectAccessGrantsEnabled { get; set; }

    [JsonPropertyName("attributes")]
    public Dictionary<string, string>? Attributes { get; set; }

    [JsonPropertyName("protocolMappers")]
    public List<KeycloakProtocolMapper>? ProtocolMappers { get; set; }
}
