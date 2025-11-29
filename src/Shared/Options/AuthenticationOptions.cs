namespace FireInvent.Shared.Options;

public class AuthenticationOptions
{
    public string Authority { get; init; } = string.Empty;

    public List<string> ValidIssuers { get; init; } = [];

    public List<string> ValidAudiences { get; init; } = [];

    public List<string> Scopes { get; init; } =
    [
        "openid",
        "profile",
        "email",
    ];
}
