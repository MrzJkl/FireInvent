using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record CreateOrUpdateOrderItemModel
{
    [Required]
    public Guid OrderId { get; init; }

    [Required]
    public Guid VariantId { get; init; }

    [Required]
    public int Quantity { get; init; }
}
