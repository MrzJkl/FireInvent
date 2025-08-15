using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record CreateClothingItemAssignmentHistoryModel
{
    [Required]
    public Guid ItemId { get; init; }

    [Required]
    public Guid PersonId { get; init; }

    [Required]
    public DateTime AssignedFrom { get; init; }

    public DateTime? AssignedUntil { get; init; }
}
