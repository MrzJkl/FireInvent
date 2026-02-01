using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

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
    
    [ForeignKey(nameof(AssignedBy))]
    public Guid? AssignedById { get; set; }

    [Required]
    public DateOnly AssignedFrom { get; set; }

    public DateOnly? AssignedUntil { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public Guid? CreatedById { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    [ForeignKey(nameof(ModifiedBy))]
    public Guid? ModifiedById { get; set; }

    [Required]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public virtual Item Item { get; set; } = null!;

    /// <summary>
    /// Items can be assigned to either a person or a storage location. Only one of these must be set.
    /// </summary>
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public virtual Person? Person { get; set; }

    /// <summary>
    /// Items can be assigned to either a person or a storage location. Only one of these must be set.
    /// </summary>
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public virtual StorageLocation? StorageLocation { get; set; }
    
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public virtual Tenant Tenant { get; set; } = null!;
    
    [DeleteBehavior(DeleteBehavior.SetNull)]
    public virtual User? CreatedBy { get; set; }
    
    [DeleteBehavior(DeleteBehavior.SetNull)]
    public virtual User? ModifiedBy { get; set; }
    
    [DeleteBehavior(DeleteBehavior.SetNull)]
    public virtual User? AssignedBy { get; set; }
}
