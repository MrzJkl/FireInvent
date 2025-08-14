using FireInvent.Contract;
using FireInvent.Database.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireInvent.Shared.Models
{
    public record OrderModel : CreateOrderModel
    {
        [Required]
        public Guid Id { get; init; }

        [Required]
        public new List<OrderItemModel> Items { get; init; } = [];
    }
}
