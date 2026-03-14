using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record CreateOrUpdateStorageLocationMinStockModel
{
    [Required]
    public Guid VariantId { get; init; }

    [Required]
    [Range(0, int.MaxValue)]
    public int MinStock { get; init; }
}
