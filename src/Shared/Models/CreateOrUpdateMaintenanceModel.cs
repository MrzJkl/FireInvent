using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record CreateOrUpdateMaintenanceModel
{
    [Required]
    public Guid ItemId { get; init; }

    [Required]
    public DateTimeOffset PerformedAt { get; init; }

    [Required]
    public Guid TypeId { get; init; }

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Remarks { get; init; }

    /// <summary>
    /// User ID from Keycloak who performed the maintenance.
    /// </summary>
    [Required]
    public Guid PerformedById { get; init; }
}
