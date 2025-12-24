using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

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
