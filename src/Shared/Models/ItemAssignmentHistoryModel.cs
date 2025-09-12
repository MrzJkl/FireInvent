using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record ItemAssignmentHistoryModel : CreateItemAssignmentHistoryModel
{
    [Required]
    public Guid Id { get; init; }
}
