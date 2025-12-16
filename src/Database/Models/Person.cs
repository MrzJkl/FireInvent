using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

[Index(nameof(FirstName), nameof(LastName), IsUnique = true)]
[Index(nameof(ExternalId), IsUnique = true)]
[Index(nameof(FirstName))]
[Index(nameof(LastName))]
public record Person : IHasTenant
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Tenant))]
    public Guid TenantId { get; set; }

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

    public virtual ICollection<ItemAssignmentHistory> AssignedItems { get; set; } = [];

    public virtual ICollection<Department> Departments { get; set; } = [];

    public virtual Tenant Tenant { get; set; } = null!;
}
