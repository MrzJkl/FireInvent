using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

[Index(nameof(Name), nameof(TenantId), IsUnique = true)]
public record Department : IHasTenant, IAuditable
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Tenant))]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Description { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }
    
    public Guid? CreatedById { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    public Guid? ModifiedById { get; set; }

    public virtual ICollection<Person> Persons { get; set; } = [];
    
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public virtual Tenant Tenant { get; set; } = null!;
    
    [DeleteBehavior(DeleteBehavior.SetNull)]
    public virtual User? CreatedBy { get; set; }
    
    [DeleteBehavior(DeleteBehavior.SetNull)]
    public virtual User? ModifiedBy { get; set; }
}
