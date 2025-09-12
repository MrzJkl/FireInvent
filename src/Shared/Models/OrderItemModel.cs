using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models
{
    public record OrderItemModel : CreateOrderItemModel
    {
        [Required]
        public Guid Id { get; init; }

        [Required]
        public VariantModel ClothingVariant { get; init; } = new();
    }
}
