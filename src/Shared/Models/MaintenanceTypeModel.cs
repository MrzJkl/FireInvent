using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record MaintenanceTypeModel : CreateOrUpdateMaintenanceTypeModel
{
    [Required]
    public Guid Id { get; init; }
}
