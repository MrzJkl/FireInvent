using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models
{
    public record OrderItem
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey(nameof(Order))]
        public Guid OrderId { get; set; }

        [Required]
        [ForeignKey(nameof(ClothingVariant))]
        public Guid ClothingVariantId { get; set; }

        [Required]
        public int Quantity { get; set; }

        public virtual Order Order { get; set; } = null!;

        public virtual ClothingVariant ClothingVariant { get; set; } = null!;
    }
}
