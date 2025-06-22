using FlameGuardLaundry.Contract;
using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Shared.Models;

public record MaintenanceModel : CreateMaintenanceModel
{
    [Required]
    public Guid Id { get; init; }
}
