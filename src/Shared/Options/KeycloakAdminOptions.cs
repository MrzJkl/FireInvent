namespace FireInvent.Shared.Options;

public class KeycloakAdminOptions
{
    public string Url { get; init; } = string.Empty;

    public string AdminUsername { get; init; } = string.Empty;

    public string AdminPassword { get; init; } = string.Empty;

    public string ApiClientPrefix { get; init; } = "api-integration-";

    public string Realm { get; init; } = "fireinvent";
}
