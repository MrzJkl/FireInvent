using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record OrderItemModel : CreateOrUpdateOrderItemModel
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    public VariantModel Variant { get; init; } = new();

    public PersonModel? Person { get; init; }
}
