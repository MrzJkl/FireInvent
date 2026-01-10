using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record MaintenanceModel : CreateOrUpdateMaintenanceModel
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    public MaintenanceTypeModel Type { get; init; } = null!;

    [Required]
    public DateTimeOffset CreatedAt { get; init; }

    [Required]
    public Guid CreatedById { get; init; }

    public DateTimeOffset? ModifiedAt { get; init; }

    public Guid? ModifiedById { get; init; }
}
