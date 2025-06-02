using FlameGuardLaundry.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FlameGuardLaundry.Database.Models;

[Index(nameof(FirstName), nameof(LastName), IsUnique = true)]
[Index(nameof(ExternalId), IsUnique = true)]
[Index(nameof(FirstName))]
[Index(nameof(LastName))]
public record Person
{
    [Key]
    public Guid Id { get; init; }

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Remarks { get; set; }

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? ContactInfo { get; set; }

    [MaxLength(ModelConstants.MaxStringLength)]
    public string? ExternalId { get; set; }

    public virtual ICollection<ClothingItemAssignmentHistory> AssignedItems { get; set; } = [];

    public virtual ICollection<Department> Departments { get; set; } = [];
}
