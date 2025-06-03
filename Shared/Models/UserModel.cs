using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Shared.Models
{
    public record UserModel
    {
        public string Id { get; init; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;

        [Required]
        public string UserName { get; init; } = string.Empty;
    }
}
