using FlameGuardLaundry.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlameGuardLaundry.Shared.Models;

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
