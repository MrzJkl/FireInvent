using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record OrderModel : CreateOrderModel
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    public new List<OrderItemModel> Items { get; init; } = [];
}
