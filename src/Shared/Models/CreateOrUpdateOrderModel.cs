using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record CreateOrUpdateOrderModel
{
    [MaxLength(ModelConstants.MaxStringLength)]
    public string? OrderIdentifier { get; init; }

    [Required]
    public DateTimeOffset OrderDate { get; init; }

    [Required]
    public OrderStatus Status { get; init; }

    public DateTimeOffset? DeliveryDate { get; init; }
}
