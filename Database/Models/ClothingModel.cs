using FlameGuardLaundry.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Database.Models;

[Index(nameof(Name), nameof(Manufacturer), IsUnique = true)]
[Index(nameof(Name))]
[Index(nameof(Manufacturer))]
public record ClothingModel 
{
    [Key]
    public Guid Id { get; init; }

    [Required]
    [MinLength(1)]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Manufacturer { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Description { get; set; }

    [Required]
    public GearType Type { get; set; }

    public virtual ICollection<ClothingVariant> Variants { get; set; } = [];
}
