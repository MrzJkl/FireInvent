using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record CreateOrUpdateProductModel
{
    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Name { get; init; } = string.Empty;

    [Required]
    public Guid ManufacturerId { get; init; }

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Description { get; init; }

    [MaxLength(ModelConstants.MaxStringLength)]
    public string? ExternalIdentifier { get; set; }

    [Required]
    public Guid TypeId { get; init; }
}
