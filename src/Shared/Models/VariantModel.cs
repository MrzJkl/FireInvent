using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record VariantModel : CreateOrUpdateVariantModel
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    public ProductModel Product { get; init; } = null!;
}
