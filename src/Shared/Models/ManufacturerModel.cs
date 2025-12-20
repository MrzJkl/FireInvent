using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models
{
    public record ManufacturerModel : CreateOrUpdateManufacturerModel
    {
        [Required]
        public Guid Id { get; set; }
    }
}
