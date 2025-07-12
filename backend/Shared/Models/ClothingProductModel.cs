using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record ClothingProductModel : CreateClothingProductModel
{
    [Required]
    public Guid Id { get; init; } = Guid.Empty;
}
