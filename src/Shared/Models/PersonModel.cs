using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record PersonModel : CreatePersonModel
{
    [Required]
    public Guid Id { get; init; }

    public List<DepartmentModel> Departments { get; init; } = [];
}
