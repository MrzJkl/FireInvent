using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models
{
    public record ProductTypeModel : CreateProductTypeModel
    {
        [Required]
        public Guid Id { get; init; }
    }
}
