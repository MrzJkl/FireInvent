using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record ItemAssignmentHistoryModel : CreateOrUpdateItemAssignmentHistoryModel
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    public PersonModel Person { get; init; } = null!;

    public UserModel? AssignedBy { get; init; }
}
