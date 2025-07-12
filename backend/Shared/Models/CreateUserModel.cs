using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record CreateUserModel
{
    [Required]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string UserName { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}
