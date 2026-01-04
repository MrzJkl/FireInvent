using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record UserModel
{
    [Required]
    public Guid Id { get; init; }

    [DataType(DataType.EmailAddress)]
    public string? EMail { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }
}
