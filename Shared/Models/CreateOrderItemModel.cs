using System.ComponentModel.DataAnnotations;

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
