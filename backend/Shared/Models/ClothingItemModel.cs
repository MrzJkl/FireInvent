using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Shared.Models;

public record ClothingItemModel : CreateClothingItemModel
{
    [Required]
    public Guid Id { get; init; }
}
