namespace FireInvent.Shared.Models;

public class ApiIntegrationModel
{
    public required string ClientId { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public bool Enabled { get; set; }
}
