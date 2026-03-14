using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

[Index(nameof(StorageLocationId), nameof(VariantId), IsUnique = true)]
public record StorageLocationMinStock : IHasTenant, IAuditable
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Tenant))]
    public Guid TenantId { get; set; }

    [Required]
    [ForeignKey(nameof(StorageLocation))]
    public Guid StorageLocationId { get; set; }

    [Required]
    [ForeignKey(nameof(Variant))]
    public Guid VariantId { get; set; }

    [Required]
    public int MinStock { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public Guid? CreatedById { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    [ForeignKey(nameof(ModifiedBy))]
    public Guid? ModifiedById { get; set; }

    [DeleteBehavior(DeleteBehavior.Cascade)]
    public virtual StorageLocation StorageLocation { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Cascade)]
    public virtual Variant Variant { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Cascade)]
    public virtual Tenant Tenant { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.SetNull)]
    public virtual User? CreatedBy { get; set; }

    [DeleteBehavior(DeleteBehavior.SetNull)]
    public virtual User? ModifiedBy { get; set; }
}
