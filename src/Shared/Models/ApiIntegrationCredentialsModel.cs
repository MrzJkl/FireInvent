using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record ApiIntegrationCredentialsModel
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    public string ClientId { get; init; } = string.Empty;

    [Required]
    public string ClientSecret { get; init; } = string.Empty;

    [Required]
    public string Name { get; init; } = string.Empty;
}
