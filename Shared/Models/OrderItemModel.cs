using System.ComponentModel.DataAnnotations;

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
