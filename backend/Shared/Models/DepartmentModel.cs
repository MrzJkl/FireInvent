using FlameGuardLaundry.Contract;
using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Shared.Models;

public record DepartmentModel : CreateDepartmentModel
{
    [Required]
    public Guid Id { get; init; }
}
