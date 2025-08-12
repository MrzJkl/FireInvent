namespace FireInvent.Shared.Options;

public class AuthenticationOptions
{
    public string Authority { get; init; }

    public string ClientIdForSwagger { get; init; } = "fireinvent-swagger";

    public List<string> Scopes { get; init; } =
    [
        "openid",
        "profile",
        "email",
    ];
}
