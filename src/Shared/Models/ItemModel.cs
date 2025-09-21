using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record ItemModel : CreateItemModel
{
    [Required]
    public Guid Id { get; init; }
}
