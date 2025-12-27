using System.ComponentModel.DataAnnotations;

namespace FireInvent.Contract
{
    public interface IAuditable
    {
        [Required]
        public DateTimeOffset CreatedAt { get; set; }

        [Required]
        public Guid CreatedById { get; set; }

        public DateTimeOffset? ModifiedAt { get; set; }

        public Guid? ModifiedById { get; set; }
    }
}
