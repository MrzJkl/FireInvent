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

    [Required]
    public DateTimeOffset CreatedAt { get; init; }

    [Required]
    public Guid CreatedById { get; init; }

    public DateTimeOffset? ModifiedAt { get; init; }

    public Guid? ModifiedById { get; init; }
}
