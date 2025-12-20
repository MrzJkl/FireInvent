using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models
{
    public record CreateOrUpdateManufacturerModel
    {
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
    }
}
