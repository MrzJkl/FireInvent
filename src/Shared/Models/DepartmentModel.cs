using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record DepartmentModel : CreateOrUpdateDepartmentModel
{
    [Required]
    public Guid Id { get; init; }
}
