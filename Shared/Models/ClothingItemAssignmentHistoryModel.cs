using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Shared.Models;

public record ClothingItemAssignmentHistoryModel
{
    public Guid Id { get; init; }

    [Required]
    public Guid ItemId { get; init; }

    [Required]
    public Guid PersonId { get; init; }

    [Required]
    public DateTime AssignedFrom { get; init; }

    public DateTime? AssignedUntil { get; init; }
}
