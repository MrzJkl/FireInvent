using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

[Index(nameof(AppointmentId), nameof(PersonId), IsUnique = true)]
public record Visit : IHasTenant, IAuditable
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Tenant))]
    public Guid TenantId { get; set; }

    [Required]
    [ForeignKey(nameof(Appointment))]
    public Guid AppointmentId { get; set; }

    [Required]
    [ForeignKey(nameof(Person))]
    public Guid PersonId { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    [Required]
    public Guid CreatedById { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    public Guid? ModifiedById { get; set; }

    [Required]
    public virtual Appointment Appointment { get; set; } = null!;

    [Required]
    public virtual Person Person { get; set; } = null!;

    [Required]
    public virtual List<VisitItem> Items { get; set; } = [];

    [Required]
    public virtual Tenant Tenant { get; set; } = null!;
}
