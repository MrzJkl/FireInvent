using FlameGuardLaundry.Contract;
using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Shared.Models;

public record CreateMaintenanceModel
{
    [Required]
    public Guid ItemId { get; init; }

    [Required]
    public DateTime PerformedAt { get; init; }

    [Required]
    public MaintenanceType MaintenanceType { get; init; }

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Remarks { get; init; }

    public string? PerformedById { get; init; }

    public UserModel? PerformedBy { get; init; }
}
