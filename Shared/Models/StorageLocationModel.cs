using FlameGuardLaundry.Contract;
using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Shared.Models;

public record StorageLocationModel
{
    public Guid Id { get; init; }

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Remarks { get; init; } = string.Empty;
}
