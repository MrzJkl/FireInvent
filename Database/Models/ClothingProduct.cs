using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Database.Models;

[Index(nameof(Name), nameof(Manufacturer), IsUnique = true)]
[Index(nameof(Name))]
[Index(nameof(Manufacturer))]
public record ClothingProduct
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Manufacturer { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Description { get; set; }

    [Required]
    public GearType Type { get; set; }

    public virtual ICollection<ClothingVariant> Variants { get; set; } = [];
}
