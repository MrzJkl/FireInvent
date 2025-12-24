using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models
{
    [Index(nameof(Name), nameof(TenantId), IsUnique = true)]
    public record Manufacturer : IHasTenant, IAuditable
    {
        [Required]
        [ForeignKey(nameof(Tenant))]
        public Guid TenantId { get; set; }

        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(ModelConstants.MaxStringLength)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(ModelConstants.MaxStringLengthLong)]
        public string? Description { get; set; }

        [MaxLength(ModelConstants.MaxStringLength)]
        public string? Street { get; set; }

        [MaxLength(ModelConstants.MaxStringLength)]
        public string? City { get; set; }

        [MaxLength(ModelConstants.MaxStringLength)]
        [DataType(DataType.PostalCode)]
        public string? PostalCode { get; set; }

        [MaxLength(ModelConstants.MaxStringLength)]
        public string? HouseNumber { get; set; }

        [MaxLength(ModelConstants.MaxStringLength)]
        public string? Country { get; set; }

        [MaxLength(ModelConstants.MaxStringLength)]
        [DataType(DataType.Url)]
        public string? Website { get; set; }

        [MaxLength(ModelConstants.MaxStringLength)]
        [DataType(DataType.PhoneNumber)]
        public string? PhoneNumber { get; set; }

        [MaxLength(ModelConstants.MaxStringLength)]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        [Required]
        public DateTimeOffset CreatedAt { get; set; }

        [Required]
        public Guid CreatedById { get; set; }

        public DateTimeOffset? ModifiedAt { get; set; }

        public Guid? ModifiedById { get; set; }

        public virtual ICollection<Product> Products { get; set; } = [];

        [Required]
        public virtual Tenant Tenant { get; set; } = null!;
    }
}
