using FlameGuardLaundry.Contract;
using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Shared.Models;

public record PersonModel
{
    public Guid Id { get; init; }

    [Required]

    [MaxLength(ModelConstants.MaxStringLength)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string LastName { get; init; } = string.Empty;

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Remarks { get; init; }

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? ContactInfo { get; init; }

    [MaxLength(ModelConstants.MaxStringLength)]
    public string? ExternalId { get; init; }
}
