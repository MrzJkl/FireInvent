using FlameGuardLaundry.Contract;
using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Shared.Models;

public record ClothingVariantModel
{
    public Guid Id { get; init; }

    [Required]
    public Guid ProductId { get; init; }

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? AdditionalSpecs { get; init; }
}
