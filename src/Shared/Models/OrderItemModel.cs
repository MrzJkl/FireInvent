using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record OrderItemModel : CreateOrUpdateOrderItemModel
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    public VariantModel Variant { get; init; } = new();

    public PersonModel? Person { get; init; }

    [Required]
    public DateTimeOffset CreatedAt { get; init; }

    [Required]
    public Guid CreatedById { get; init; }

    public DateTimeOffset? ModifiedAt { get; init; }

    public Guid? ModifiedById { get; init; }
}
