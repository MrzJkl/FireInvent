using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record CreateOrUpdateItemModel
{
    [Required]
    public Guid VariantId { get; init; }

    [MaxLength(ModelConstants.MaxStringLength)]
    public string? Identifier { get; init; }

    public Guid? StorageLocationId { get; init; }

    [Required]
    public ItemCondition Condition { get; init; }

    [Required]
    public DateTimeOffset PurchaseDate { get; init; }

    public DateTimeOffset? RetirementDate { get; init; }
}
