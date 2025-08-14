using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

[Index(nameof(ItemId), nameof(AssignedFrom), IsUnique = true)]
public record ClothingItemAssignmentHistory
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Item))]
    public Guid ItemId { get; set; }

    [Required]
    [ForeignKey(nameof(Person))]
    public Guid PersonId { get; set; }

    [ForeignKey(nameof(AssignedBy))]
    public Guid? AssignedById { get; set; }

    [Required]
    public DateTime AssignedFrom { get; set; }

    public DateTime? AssignedUntil { get; set; }

    public virtual ClothingItem Item { get; set; } = null!;

    public virtual Person Person { get; set; } = null!;

    public virtual User? AssignedBy { get; set; } = null!;
}
