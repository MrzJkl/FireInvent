using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record MaintenanceModel : CreateMaintenanceModel
{
    [Required]
    public Guid Id { get; init; }
}
