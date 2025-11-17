using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record MaintenanceModel : CreateOrUpdateMaintenanceModel
{
    [Required]
    public Guid Id { get; init; }

    public UserModel? PerformedBy { get; init; }

    [Required]
    public MaintenanceTypeModel Type { get; init; } = null!;
}
