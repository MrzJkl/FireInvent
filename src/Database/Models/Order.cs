using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Database.Models;

public record Order : IHasTenant
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string TenantId { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MaxStringLength)]
    public string? OrderIdentifier { get; set; }

    [Required]
    public DateTimeOffset OrderDate { get; set; }

    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Draft;

    public DateTimeOffset? DeliveryDate { get; set; }

    public virtual ICollection<OrderItem> Items { get; set; } = [];
}
