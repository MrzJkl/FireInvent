using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

[Index(nameof(Name), nameof(TenantId), IsUnique = true)]
public record StorageLocation : IHasTenant
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Tenant))]
    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Name { get; set; } = string.Empty;

    public string? Remarks { get; set; } = string.Empty;

    public virtual ICollection<Item> StoredItems { get; set; } = [];

    [Required]
    public virtual Tenant Tenant { get; set; } = null!;
}
