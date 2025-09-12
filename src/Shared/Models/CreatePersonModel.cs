using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record CreatePersonModel
{
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
