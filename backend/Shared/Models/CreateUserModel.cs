namespace FlameGuardLaundry.Shared.Models;

public record CreateUserModel
{
    public string Email { get; init; } = string.Empty;

    public string UserName { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
