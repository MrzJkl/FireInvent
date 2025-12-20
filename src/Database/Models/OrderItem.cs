using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

public record OrderItem : IHasTenant
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Tenant))]
    public Guid TenantId { get; set; }

    [Required]
    [ForeignKey(nameof(Order))]
    public Guid OrderId { get; set; }

    [Required]
    [ForeignKey(nameof(Variant))]
    public Guid VariantId { get; set; }

    [ForeignKey(nameof(Person))]
    public Guid? PersonId { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public virtual Order Order { get; set; } = null!;

    [Required]
    public virtual Variant Variant { get; set; } = null!;

    [Required]
    public virtual Tenant Tenant { get; set; } = null!;

    public virtual Person? Person { get; set; }
}
