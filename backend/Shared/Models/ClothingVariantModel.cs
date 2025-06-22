using FlameGuardLaundry.Contract;
using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Shared.Models;

public record ClothingVariantModel : CreateClothingVariantModel
{
    [Required]
    public Guid Id { get; init; }
}
