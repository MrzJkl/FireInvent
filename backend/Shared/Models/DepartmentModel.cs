using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record DepartmentModel : CreateDepartmentModel
{
    [Required]
    public Guid Id { get; init; }
}
