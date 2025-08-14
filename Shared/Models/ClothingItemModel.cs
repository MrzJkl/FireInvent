using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record ClothingItemModel : CreateClothingItemModel
{
    [Required]
    public Guid Id { get; init; }
}
