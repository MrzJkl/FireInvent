using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record StorageLocationModel : CreateStorageLocationModel
{
    [Required]
    public Guid Id { get; init; }
}
