using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record CreateOrUpdateItemAssignmentHistoryModel
{
    [Required]
    public Guid ItemId { get; init; }

    [Required]
    public Guid PersonId { get; init; }

    /// <summary>
    /// User ID from Keycloak who assigned the item.
    /// </summary>
    [Required]
    public Guid AssignedById { get; init; }

    [Required]
    public DateOnly AssignedFrom { get; init; }

    public DateOnly? AssignedUntil { get; init; }
}
