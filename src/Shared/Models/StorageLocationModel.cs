using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record StorageLocationModel : CreateOrUpdateStorageLocationModel
{
    [Required]
    public Guid Id { get; init; }
}
