using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Database.Models
{
    [Index(nameof(EMail), IsUnique = true)]
    public record User
    {
        [Key]
        public Guid Id { get; init; }

        [MaxLength(ModelConstants.MaxStringLength)]
        public string EMail { get; set; } = string.Empty;

        [MaxLength(ModelConstants.MaxStringLength)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(ModelConstants.MaxStringLength)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        public DateTime? LastLogin { get; set; }
    }
}
