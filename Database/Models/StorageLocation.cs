using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Database.Models;

[Index(nameof(Name), IsUnique = true)]
public record StorageLocation
{
    [Key]
    public Guid Id { get; init; }

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;

    public string? Remarks { get; set; } = string.Empty;

    public virtual ICollection<ClothingItem> StoredItems { get; set; } = [];
}
