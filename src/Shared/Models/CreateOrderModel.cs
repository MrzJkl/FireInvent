using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models
{
    public record CreateOrderModel
    {
        [MaxLength(ModelConstants.MaxStringLength)]
        public string? OrderIdentifier { get; init; }

        [Required]
        public DateTimeOffset OrderDate { get; init; }

        [Required]
        public OrderStatus Status { get; init; }

        [Required]
        public List<CreateOrderItemModel> Items { get; init; } = [];

        public DateTimeOffset? DeliveryDate { get; init; }
    }
}
