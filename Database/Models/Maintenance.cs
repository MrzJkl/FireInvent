using FlameGuardLaundry.Contract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlameGuardLaundry.Database.Models;

public record Maintenance
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Item))]
    public Guid ItemId { get; set; }

    [Required]
    public DateTime Performed { get; set; }

    [Required]
    public MaintenanceType MaintenanceType { get; set; }

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Remarks { get; set; }

    public virtual ClothingItem Item { get; set; } = null!;
}
