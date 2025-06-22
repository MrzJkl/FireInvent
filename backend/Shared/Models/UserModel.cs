using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Shared.Models;

public record UserModel : CreateUserModel
{
    [Required]
    public string Id { get; init; } = string.Empty;
}
