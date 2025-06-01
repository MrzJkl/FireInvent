using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Database.Models;

[Index(nameof(Name), IsUnique = true)]
[Index(nameof(Name))]
public record Department
{
    [Key]
    public Guid Id { get; init; }

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Description { get; set; }

    public virtual ICollection<Person> Persons { get; set; } = [];
}
