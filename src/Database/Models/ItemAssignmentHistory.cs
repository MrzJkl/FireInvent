using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

[Index(nameof(ItemId), nameof(AssignedFrom), nameof(TenantId), IsUnique = true)]
public record ItemAssignmentHistory : IHasTenant, IAuditable
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Tenant))]
    public Guid TenantId { get; set; }

    [Required]
    [ForeignKey(nameof(Item))]
    public Guid ItemId { get; set; }

    /// <summary>
    /// Items can be assigned to either a person or a storage location. Only one of these must be set.
    /// </summary>
    [ForeignKey(nameof(Person))]
    public Guid? PersonId { get; set; }

    /// <summary>
    /// Items can be assigned to either a person or a storage location. Only one of these must be set.
    /// </summary>
    [ForeignKey(nameof(StorageLocation))]
    public Guid? StorageLocationId { get; set; }

    /// <summary>
    /// User ID from Keycloak who assigned the item.
    /// </summary>
    [Required]
    public Guid AssignedById { get; set; }

    [Required]
    public DateOnly AssignedFrom { get; set; }

    public DateOnly? AssignedUntil { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    public Guid? CreatedById { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    public Guid? ModifiedById { get; set; }

    [Required]
    public virtual Item Item { get; set; } = null!;

    /// <summary>
    /// Items can be assigned to either a person or a storage location. Only one of these must be set.
    /// </summary>
    public virtual Person? Person { get; set; }

    /// <summary>
    /// Items can be assigned to either a person or a storage location. Only one of these must be set.
    /// </summary>
    public virtual StorageLocation? StorageLocation { get; set; }

    [Required]
    public virtual Tenant Tenant { get; set; } = null!;
    
    public virtual User? CreatedBy { get; set; }
    
    public virtual User? ModifiedBy { get; set; }
}
