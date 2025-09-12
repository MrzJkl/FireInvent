using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record CreateItemModel
{
    [Required]
    public Guid VariantId { get; init; }

    [MaxLength(ModelConstants.MaxStringLength)]
    public string? Identifier { get; init; } = string.Empty;

    public Guid? StorageLocationId { get; init; }

    [Required]
    public ItemCondition Condition { get; init; }

    [Required]
    public DateTimeOffset PurchaseDate { get; init; }

    public DateTimeOffset? RetirementDate { get; init; }
}
