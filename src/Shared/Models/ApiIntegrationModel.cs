using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record ApiIntegrationModel : CreateApiIntegrationModel
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string ClientId { get; init; } = string.Empty;

    public bool Enabled { get; init; }
}
