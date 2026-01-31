﻿using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models
{
    public record VisitModel : CreateOrUpdateVisitModel
    {
        [Required]
        public Guid Id { get; init; }

        [Required]
        public AppointmentModel Appointment { get; init; } = null!;

        [Required]
        public PersonModel Person { get; init; } = null!;

        [Required]
        public DateTimeOffset CreatedAt { get; init; }

        public UserModel? CreatedBy { get; init; }

        public DateTimeOffset? ModifiedAt { get; init; }

        public UserModel? ModifiedBy { get; init; }
    }
}
