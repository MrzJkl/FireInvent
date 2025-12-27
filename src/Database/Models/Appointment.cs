using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

public record Appointment : IHasTenant, IAuditable
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Tenant))]
    public Guid TenantId { get; set; }

    [Required]
    public DateTimeOffset ScheduledAt { get; set; }

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Description { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    [Required]
    public Guid CreatedById { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    public Guid? ModifiedById { get; set; }

    [Required]
    public virtual ICollection<Visit> Visits { get; set; } = [];

    [Required]
    public virtual Tenant Tenant { get; set; } = null!;
}
