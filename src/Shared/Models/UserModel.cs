using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record UserModel
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    public string EMail { get; set; } = string.Empty;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }
}
