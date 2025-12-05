namespace FireInvent.Shared.Options;

public class CorsOptions
{
    public bool Enabled { get; init; } = true;

    public List<string> AllowedOrigins { get; init; } = [];

    public List<string> AllowedMethods { get; init; } = ["GET", "POST", "PUT", "DELETE", "OPTIONS"];

    public List<string> AllowedHeaders { get; init; } = ["Authorization", "Content-Type"];

    public bool AllowCredentials { get; init; } = false;
}
