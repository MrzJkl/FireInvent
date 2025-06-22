namespace FlameGuardLaundry.Shared.Options;

public class JwtOptions
{
    public string Key { get; init; }

    public string Issuer { get; init; }

    public string Audience { get; init; }

    public int ExpireHours { get; init; }
}
