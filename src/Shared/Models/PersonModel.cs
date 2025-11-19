using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record PersonModel : CreateOrUpdatePersonModel
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    public List<DepartmentModel> Departments { get; init; } = [];
}
