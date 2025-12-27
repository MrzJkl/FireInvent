using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

[Index(nameof(Identifier), nameof(TenantId), IsUnique = true)]
public record Item : IHasTenant, IAuditable
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Tenant))]
    public Guid TenantId { get; set; }

    [Required]
    [ForeignKey(nameof(Variant))]
    public Guid VariantId { get; set; }

    [MaxLength(ModelConstants.MaxStringLength)]
    public string? Identifier { get; set; }

    [Required]
    public bool IsDemoItem { get; set; }

    [Required]
    public ItemCondition Condition { get; set; }

    [Required]
    public DateOnly PurchaseDate { get; set; }

    public DateOnly? RetirementDate { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    [Required]
    public Guid CreatedById { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    public Guid? ModifiedById { get; set; }

    [Required]
    public virtual Variant Variant { get; set; } = null!;

    [Required]
    public virtual Tenant Tenant { get; set; } = null!;

    public virtual ICollection<ItemAssignmentHistory> Assignments { get; set; } = [];

    public virtual ICollection<Maintenance> Maintenances { get; set; } = [];
}
