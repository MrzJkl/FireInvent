using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlameGuardLaundry.Database.Models;

[Index(nameof(ModelId), nameof(Name), IsUnique = true)]
[Index(nameof(Name))]
public record ClothingVariant
{
    [Key]
    public Guid Id { get; init; }

    [Required]
    [ForeignKey(nameof(Model))]
    public Guid ModelId { get; set; }

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? AdditionalSpecs { get; set; }

    public virtual ClothingModel Model { get; set; } = null!;

    public virtual ICollection<ClothingItem> Items { get; set; } = [];
}
