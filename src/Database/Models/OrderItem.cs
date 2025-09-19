using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

public record OrderItem
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Order))]
    public Guid OrderId { get; set; }

    [Required]
    [ForeignKey(nameof(Variant))]
    public Guid VariantId { get; set; }

    [Required]
    public int Quantity { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Variant Variant { get; set; } = null!;
}
