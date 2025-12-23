using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

public record Order : IHasTenant
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Tenant))]
    public Guid TenantId { get; set; }

    [MaxLength(ModelConstants.MaxStringLength)]
    public string? OrderIdentifier { get; set; }

    [Required]
    public DateOnly OrderDate { get; set; }

    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Draft;

    public DateOnly? DeliveryDate { get; set; }

    public virtual ICollection<OrderItem> Items { get; set; } = [];

    [Required]
    public virtual Tenant Tenant { get; set; } = null!;
}
