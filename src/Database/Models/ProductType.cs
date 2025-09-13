using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Database.Models;

[Index(nameof(Name), IsUnique = true)]
public record ProductType
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(ModelConstants.MaxStringLength)]
    [Required]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Description { get; set; }
}
