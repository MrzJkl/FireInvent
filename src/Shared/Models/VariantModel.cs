﻿using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record VariantModel : CreateVariantModel
{
    [Required]
    public Guid Id { get; init; }
}
