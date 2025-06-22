using FlameGuardLaundry.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public UserModel PerformedBy { get; init; } = new UserModel();
}
