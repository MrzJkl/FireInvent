using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record CreateClothingItemModel
{
    [Required]
    public Guid VariantId { get; init; }

    [MaxLength(ModelConstants.MaxStringLength)]
    public string? Identifier { get; init; } = string.Empty;

    public Guid? StorageLocationId { get; init; }

    [Required]
    public GearCondition Condition { get; init; }

    [Required]
    public DateTime PurchaseDate { get; init; }

    public DateTime? RetirementDate { get; init; }
}
