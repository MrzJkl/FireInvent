using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record CreateItemAssignmentHistoryModel
{
    [Required]
    public Guid ItemId { get; init; }

    [Required]
    public Guid PersonId { get; init; }

    [Required]
    public DateTimeOffset AssignedFrom { get; init; }

    public DateTimeOffset? AssignedUntil { get; init; }

    public Guid? AssignedById { get; init; }
}
