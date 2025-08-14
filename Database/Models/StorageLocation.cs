using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Database.Models;

[Index(nameof(Name), IsUnique = true)]
public record StorageLocation
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Name { get; set; } = string.Empty;

    public string? Remarks { get; set; } = string.Empty;

    public virtual ICollection<ClothingItem> StoredItems { get; set; } = [];
}
