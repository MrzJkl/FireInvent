using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

public record Maintenance
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Item))]
    public Guid ItemId { get; set; }

    [Required]
    public DateTimeOffset PerformedAt { get; set; }

    [ForeignKey(nameof(PerformedBy))]
    public Guid? PeformedById { get; set; }

    [Required]
    public MaintenanceType MaintenanceType { get; set; }

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Remarks { get; set; }

    public virtual ClothingItem Item { get; set; } = null!;

    public virtual User? PerformedBy { get; set; } = null!;
}
