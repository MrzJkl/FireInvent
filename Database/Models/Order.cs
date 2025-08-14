using FireInvent.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireInvent.Database.Models
{
    public record Order
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(ModelConstants.MaxStringLength)]
        public string? OrderIdentifier { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Draft;

        public DateTime? DeliveryDate { get; set; }

        public virtual ICollection<OrderItem> Items { get; set; } = [];
    }
}
