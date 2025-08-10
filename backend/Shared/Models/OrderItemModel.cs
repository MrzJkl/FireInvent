using FireInvent.Database.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireInvent.Shared.Models
{
    public record OrderItemModel : CreateOrderItemModel
    {
        [Required]
        public Guid Id { get; init; }

        [Required]
        public ClothingVariantModel ClothingVariant { get; init; } = new();
    }
}
