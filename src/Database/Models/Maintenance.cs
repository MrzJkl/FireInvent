using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

public record Maintenance : IHasTenant, IAuditable
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Tenant))]
    public Guid TenantId { get; set; }

    [Required]
    [ForeignKey(nameof(Item))]
    public Guid ItemId { get; set; }

    [Required]
    [ForeignKey(nameof(Type))]
    public Guid TypeId { get; set; }

    [Required]
    public DateTimeOffset PerformedAt { get; set; }

    /// <summary>
    /// User ID from Keycloak who performed the maintenance.
    /// </summary>
    [Required]
    public Guid PerformedById { get; set; }

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Remarks { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    [Required]
    public Guid CreatedById { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    public Guid? ModifiedById { get; set; }

    [Required]
    public virtual MaintenanceType Type { get; set; } = null!;

    [Required]
    public virtual Item Item { get; set; } = null!;

    [Required]
    public virtual Tenant Tenant { get; set; } = null!;
}
