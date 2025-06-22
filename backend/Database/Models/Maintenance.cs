using FlameGuardLaundry.Contract;
using Microsoft.AspNetCore.Identity;
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
    public DateTime PerformedAt { get; set; }

    [ForeignKey(nameof(PerformedBy))]
    public string? PeformedById { get; set; } = string.Empty;

    [Required]
    public MaintenanceType MaintenanceType { get; set; }

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Remarks { get; set; }

    public virtual ClothingItem Item { get; set; } = null!;

    public virtual IdentityUser? PerformedBy { get; set; } = null!;
}
