using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Database.Models;

[Index(nameof(Realm), IsUnique = true)]
[Index(nameof(Name),IsUnique = true)]
public record Tenant
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Realm { get; set; } = string.Empty;

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Description { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
