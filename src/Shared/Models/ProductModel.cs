using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record ProductModel : CreateOrUpdateProductModel
{
    [Required]
    public Guid Id { get; init; } = Guid.Empty;

    [Required]
    public ProductTypeModel Type { get; init; } = null!;

    [Required]
    public ManufacturerModel Manufacturer { get; init; } = null!;
}
