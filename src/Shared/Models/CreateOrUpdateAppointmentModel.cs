using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record CreateOrUpdateAppointmentModel
{
    [Required]
    public DateTimeOffset ScheduledAt { get; init; }

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Description { get; init; }
}
