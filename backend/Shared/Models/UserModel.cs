using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record UserModel
{
    [Required]
    public string Id { get; init; } = string.Empty;

    [Required]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string UserName { get; init; } = string.Empty;
}
