using FlameGuardLaundry.Contract;
using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Shared.Models;

public record StorageLocationModel : CreateStorageLocationModel
{
    [Required]
    public Guid Id { get; init; }
}
