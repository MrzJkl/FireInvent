namespace FireInvent.Shared.Options;

public class OpenTelemetryOptions
{
    public bool Enabled { get; init; }
    
    public string ServiceName { get; init; } = "FireInvent.Api";
    
    public string? OtlpEndpoint { get; init; }
}