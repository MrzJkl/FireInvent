using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Shared.Models;

public record ClothingProductModel : CreateClothingProductModel
{
    [Required]
    public Guid Id { get; init; } = Guid.Empty;
}
