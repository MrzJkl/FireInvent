using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Shared.Models;

public record PersonModel : CreatePersonModel
{
    [Required]
    public Guid Id { get; init; }
}
