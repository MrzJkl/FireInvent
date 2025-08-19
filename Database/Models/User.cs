using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Database.Models
{
    [Index(nameof(EMail), IsUnique = true)]
    public record User
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(ModelConstants.MaxStringLength)]
        public string EMail { get; set; } = string.Empty;

        [MaxLength(ModelConstants.MaxStringLength)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(ModelConstants.MaxStringLength)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTimeOffset? LastLogin { get; set; }
    }
}
