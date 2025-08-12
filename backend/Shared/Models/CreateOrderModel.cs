using FireInvent.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireInvent.Shared.Models
{
    public record CreateOrderModel
    {
        [MaxLength(ModelConstants.MaxStringLength)]
        public string? OrderIdentifier { get; init; }

        [Required]
        public DateTime OrderDate { get; init; }

        [Required]
        public OrderStatus Status { get; init; }

        [Required]
        public List<CreateOrderItemModel> Items { get; init; } = [];

        public DateTime? DeliveryDate { get; init; }
    }
}
