using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record ProductTypeModel : CreateOrUpdateProductTypeModel
{
    [Required]
    public Guid Id { get; init; }
}
