using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireInvent.Shared.Models
{
    public record CreateOrderItemModel
    {
        [Required]
        public Guid OrderId { get; init; }

        [Required]
        public Guid ClothingVariantId { get; init; }

        [Required]
        public int Quantity { get; init; }
    }
}
