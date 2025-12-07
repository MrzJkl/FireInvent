namespace FireInvent.Shared.Models;

public class ApiIntegrationCredentialsModel
{
    public required string ClientId { get; set; }

    public required string ClientSecret { get; set; }

    public required string Name { get; set; }
}
