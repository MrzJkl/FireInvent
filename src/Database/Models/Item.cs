using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

[Index(nameof(Identifier), IsUnique = true)]
public record Item : IHasTenant
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string TenantId { get; set; } = string.Empty;

    [Required]
    [ForeignKey(nameof(Variant))]
    public Guid VariantId { get; set; }

    [MaxLength(ModelConstants.MaxStringLength)]
    public string? Identifier { get; set; }

    [ForeignKey(nameof(StorageLocation))]
    public Guid? StorageLocationId { get; set; }

    [Required]
    public ItemCondition Condition { get; set; }

    [Required]
    public DateTimeOffset PurchaseDate { get; set; }

    public DateTimeOffset? RetirementDate { get; set; }

    public virtual Variant Variant { get; set; } = null!;

    public virtual StorageLocation? StorageLocation { get; set; }

    public virtual ICollection<ItemAssignmentHistory> Assignments { get; set; } = [];

    public virtual ICollection<Maintenance> Maintenances { get; set; } = [];
}
