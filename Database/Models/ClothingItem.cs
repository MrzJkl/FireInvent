using FlameGuardLaundry.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlameGuardLaundry.Database.Models;

[Index(nameof(Identifier), IsUnique = true)]
public record ClothingItem
{
    [Key]
    public Guid Id { get; init; }

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
    public DateTime PurchaseDate { get; set; }

    public DateTime? RetirementDate { get; set; }

    public virtual ClothingVariant Variant { get; set; } = null!;

    public virtual StorageLocation? StorageLocation { get; set; }

    public virtual ICollection<ClothingItemAssignmentHistory> Assignments { get; set; } = [];

    public virtual ICollection<Maintenance> Maintenances { get; set; } = [];
}
