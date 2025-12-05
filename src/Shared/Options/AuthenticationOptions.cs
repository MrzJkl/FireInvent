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

    public bool RequireHttpsMetadata { get; init; } = true;

    public bool ValidateLifetime { get; init; } = true;

    public int ClockSkewSeconds { get; init; } = 60;
}
