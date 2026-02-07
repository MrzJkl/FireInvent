using System.ComponentModel.DataAnnotations;
using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Database.Models;

[Index(nameof(UserName), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public record User
{
    [Key]
    [Required]
    public Guid Id { get; set; }
    
    [MaxLength(ModelConstants.MaxStringLength)]
    public string? FirstName { get; set; }
    
    [MaxLength(ModelConstants.MaxStringLength)]
    public string? LastName { get; set; }

    [MaxLength(ModelConstants.MaxStringLength)]
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string UserName { get; set; } = string.Empty;
    
    public virtual ICollection<Tenant> Tenants { get; set; } = [];
}