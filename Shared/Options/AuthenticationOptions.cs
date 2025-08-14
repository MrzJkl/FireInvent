namespace FireInvent.Shared.Options;

public class AuthenticationOptions
{
    public string Authority { get; init; } = string.Empty;

    public string OidcDiscoveryUrlForSwagger { get; init; } = string.Empty;

    public string ClientIdForSwagger { get; init; } = "fireinvent-swagger";

    public List<string> Scopes { get; init; } =
    [
        "openid",
        "profile",
        "email",
    ];
}
