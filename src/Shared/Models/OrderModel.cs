using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record OrderModel : CreateOrUpdateOrderModel
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    public new List<OrderItemModel> Items { get; init; } = [];
}
