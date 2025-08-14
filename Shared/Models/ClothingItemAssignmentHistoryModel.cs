using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record ClothingItemAssignmentHistoryModel : CreateClothingItemAssignmentHistoryModel
{
    [Required]
    public Guid Id { get; init; }
}
