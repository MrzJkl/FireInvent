using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record StockWarningModel
{
    [Required]
    public Guid StorageLocationId { get; init; }

    [Required]
    public string StorageLocationName { get; init; } = string.Empty;

    [Required]
    public Guid VariantId { get; init; }

    [Required]
    public string VariantName { get; init; } = string.Empty;

    [Required]
    public string ProductName { get; init; } = string.Empty;

    [Required]
    public int CurrentStock { get; init; }

    [Required]
    public int MinStock { get; init; }
}
