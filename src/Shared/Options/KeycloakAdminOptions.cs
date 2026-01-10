namespace FireInvent.Shared.Options;

public class KeycloakAdminOptions
{
    public string Url { get; init; } = string.Empty;

    public string AdminClientId { get; init; } = string.Empty;

    public string AdminClientSecret { get; init; } = string.Empty;

    public string Realm { get; init; } = "fireinvent";
}
