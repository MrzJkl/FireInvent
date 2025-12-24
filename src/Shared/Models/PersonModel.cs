using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record PersonModel : CreateOrUpdatePersonModel
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    public List<DepartmentModel> Departments { get; init; } = [];

    [Required]
    public DateTimeOffset CreatedAt { get; init; }

    [Required]
    public Guid CreatedById { get; init; }

    public DateTimeOffset? ModifiedAt { get; init; }

    public Guid? ModifiedById { get; init; }
}
