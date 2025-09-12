using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

[Index(nameof(Name), nameof(Manufacturer), IsUnique = true)]
public record Product
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(Type))]
    public Guid TypeId { get; set; }

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Manufacturer { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Description { get; set; }

    [Required]
    public virtual ProductType Type { get; set; } = null!;

    public virtual ICollection<Variant> Variants { get; set; } = [];
}
