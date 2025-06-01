using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlameGuardLaundry.Database.Models
{
    [Index(nameof(ItemId), nameof(AssignedFrom), IsUnique = true)]
    public record ClothingItemAssignmentHistory
    {
        [Key]
        public Guid Id { get; init; }

        [Required]
        [ForeignKey(nameof(Item))]
        public Guid ItemId { get; init; }

        [Required]
        [ForeignKey(nameof(Person))]
        public Guid PersonId { get; init; }

        [Required]
        public DateTime AssignedFrom { get; init; }

        public DateTime? AssignedUntil { get; init; }

        public virtual ClothingItem Item { get; init; } = null!;

        public virtual Person Person { get; init; } = null!;
    }
}
