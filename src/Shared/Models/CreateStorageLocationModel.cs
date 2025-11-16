using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record CreateStorageLocationModel
{
    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Remarks { get; init; }
}
