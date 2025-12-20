using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

public record VisitItem : IHasTenant
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [ForeignKey(nameof(Visit))]
    public Guid VisitId { get; set; }

    [Required]
    [ForeignKey(nameof(Product))]
    public Guid ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    public virtual Visit Visit { get; set; } = null!;

    [Required]
    public virtual Product Product { get; set; } = null!;

    [Required]
    public virtual Tenant Tenant { get; set; } = null!;
}
