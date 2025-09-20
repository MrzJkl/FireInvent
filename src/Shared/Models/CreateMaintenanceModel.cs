using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record CreateMaintenanceModel
{
    [Required]
    public Guid ItemId { get; init; }

    [Required]
    public DateTimeOffset PerformedAt { get; init; }

    [Required]
    public Guid TypeId { get; init; }

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Remarks { get; init; }

    public Guid? PerformedById { get; init; }
}
