using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record CreateOrUpdateItemModel
{
    [Required]
    public Guid VariantId { get; init; }

    [MaxLength(ModelConstants.MaxStringLength)]
    public string? Identifier { get; init; }

    [Required]
    public ItemCondition Condition { get; init; }

    [Required]
    public DateOnly PurchaseDate { get; init; }

    [Required]
    public bool IsDemoItem { get; init; }

    public DateOnly? RetirementDate { get; init; }
}
