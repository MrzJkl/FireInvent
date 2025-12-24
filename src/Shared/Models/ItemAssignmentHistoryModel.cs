using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record ItemAssignmentHistoryModel : CreateOrUpdateItemAssignmentHistoryModel
{
    [Required]
    public Guid Id { get; init; }

    [Description("Items can be assigned to either a person or a storage location. Only one of these must be set.")]
    public PersonModel? Person { get; init; }

    [Description("Items can be assigned to either a person or a storage location. Only one of these must be set.")]
    public StorageLocationModel? StorageLocation { get; init; }

    [Required]
    public ItemModel Item { get; init; } = null!;

    [Required]
    public DateTimeOffset CreatedAt { get; init; }

    [Required]
    public Guid CreatedById { get; init; }

    public DateTimeOffset? ModifiedAt { get; init; }

    public Guid? ModifiedById { get; init; }
}
