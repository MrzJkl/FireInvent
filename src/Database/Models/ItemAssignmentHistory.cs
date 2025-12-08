using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

[Index(nameof(ItemId), nameof(AssignedFrom), IsUnique = true)]
public record ItemAssignmentHistory : IHasTenant
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string TenantId { get; set; } = string.Empty;

    [Required]
    [ForeignKey(nameof(Item))]
    public Guid ItemId { get; set; }

    [Required]
    [ForeignKey(nameof(Person))]
    public Guid PersonId { get; set; }

    [ForeignKey(nameof(AssignedBy))]
    public Guid? AssignedById { get; set; }

    [Required]
    public DateTimeOffset AssignedFrom { get; set; }

    public DateTimeOffset? AssignedUntil { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual Person Person { get; set; } = null!;

    public virtual User? AssignedBy { get; set; } = null!;
}
