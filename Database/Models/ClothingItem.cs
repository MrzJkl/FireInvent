using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

[Index(nameof(Identifier), IsUnique = true)]
public record ClothingItem
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Variant))]
    public Guid VariantId { get; set; }

    [MaxLength(ModelConstants.MaxStringLength)]
    public string? Identifier { get; set; } = string.Empty;

    [ForeignKey(nameof(StorageLocation))]
    public Guid? StorageLocationId { get; set; }

    [Required]
    public GearCondition Condition { get; set; }

    [Required]
    public DateTimeOffset PurchaseDate { get; set; }

    public DateTimeOffset? RetirementDate { get; set; }

    public virtual ClothingVariant Variant { get; set; } = null!;

    public virtual StorageLocation? StorageLocation { get; set; }

    public virtual ICollection<ClothingItemAssignmentHistory> Assignments { get; set; } = [];

    public virtual ICollection<Maintenance> Maintenances { get; set; } = [];
}
