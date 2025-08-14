using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record ClothingVariantModel : CreateClothingVariantModel
{
    [Required]
    public Guid Id { get; init; }
}
