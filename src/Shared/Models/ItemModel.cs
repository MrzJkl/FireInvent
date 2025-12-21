using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record ItemModel : CreateOrUpdateItemModel
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    public VariantModel Variant { get; init; } = null!;

    public StorageLocationModel? StorageLocation { get; init; }
}
