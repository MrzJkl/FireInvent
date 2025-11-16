using FireInvent.Database.Models;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record ProductModel : CreateProductModel
{
    [Required]
    public Guid Id { get; init; } = Guid.Empty;


    [Required]
    public ProductTypeModel Type { get; init; } = null!;
}
