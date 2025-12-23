using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

[Index(nameof(ItemId), nameof(AssignedFrom), nameof(TenantId), IsUnique = true)]
public record ItemAssignmentHistory : IHasTenant
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
    [ForeignKey(nameof(Person))]
    public Guid PersonId { get; set; }

    /// <summary>
    /// User ID from Keycloak who assigned the item.
    /// </summary>
    [Required]
    public Guid AssignedById { get; set; }

    [Required]
    public DateOnly AssignedFrom { get; set; }

    public DateOnly? AssignedUntil { get; set; }

    [Required]
    public virtual Item Item { get; set; } = null!;

    [Required]
    public virtual Person Person { get; set; } = null!;

    [Required]
    public virtual Tenant Tenant { get; set; } = null!;
}
