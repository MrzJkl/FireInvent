using FlameGuardLaundry.Contract;
using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Shared.Models;

public record ClothingItemModel
{
    public Guid Id { get; init; }

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
