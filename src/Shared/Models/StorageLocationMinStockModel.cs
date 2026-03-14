using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record StorageLocationMinStockModel
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    public Guid StorageLocationId { get; init; }

    [Required]
    public Guid VariantId { get; init; }

    [Required]
    public string VariantName { get; init; } = string.Empty;

    [Required]
    public string ProductName { get; init; } = string.Empty;

    [Required]
    public int MinStock { get; init; }

    [Required]
    public int CurrentStock { get; init; }

    public bool IsBelow => CurrentStock < MinStock;

    [Required]
    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? ModifiedAt { get; init; }
}
