using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Shared.Models;

public record ClothingItemAssignmentHistoryModel : CreateClothingItemAssignmentHistoryModel
{
    [Required]
    public Guid Id { get; init; }
}
