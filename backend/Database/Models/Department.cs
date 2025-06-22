using FlameGuardLaundry.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Database.Models;

[Index(nameof(Name), IsUnique = true)]
[Index(nameof(Name))]
public record Department
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Description { get; set; }

    public virtual ICollection<Person> Persons { get; set; } = [];
}
