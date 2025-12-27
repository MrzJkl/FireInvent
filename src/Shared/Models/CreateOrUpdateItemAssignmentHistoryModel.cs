using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record CreateOrUpdateItemAssignmentHistoryModel
{
    [Required]
    public Guid ItemId { get; init; }

    [Description("Items can be assigned to either a person or a storage location. Only one of these must be set.")]
    public Guid? PersonId { get; init; }

    [Description("Items can be assigned to either a person or a storage location. Only one of these must be set.")]
    public Guid? StorageLocationId { get; init; }

    /// <summary>
    /// User ID from Keycloak who assigned the item.
    /// </summary>
    [Required]
    public Guid AssignedById { get; init; }

    [Required]
    public DateOnly AssignedFrom { get; init; }

    public DateOnly? AssignedUntil { get; init; }
}
