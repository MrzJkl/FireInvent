using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record UserModel
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    public string EMail { get; set; } = string.Empty;

    [Required]
    public string? FirstName { get; set; }

    [Required]
    public string? LastName { get; set; }
}
