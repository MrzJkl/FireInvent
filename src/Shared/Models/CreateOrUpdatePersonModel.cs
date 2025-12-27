using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record CreateOrUpdatePersonModel
{
    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string LastName { get; init; } = string.Empty;

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Remarks { get; init; }

    [MaxLength(ModelConstants.MaxStringLength)]
    [DataType(DataType.EmailAddress)]
    public string? EMail { get; init; }

    [MaxLength(ModelConstants.MaxStringLength)]
    public string? ExternalId { get; init; }

    public List<Guid> DepartmentIds { get; init; } = [];
}
