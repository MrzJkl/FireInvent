﻿using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record ProductModel : CreateOrUpdateProductModel
{
    [Required]
    public Guid Id { get; init; } = Guid.Empty;

    [Required]
    public ProductTypeModel Type { get; init; } = null!;

    [Required]
    public ManufacturerModel Manufacturer { get; init; } = null!;

    [Required]
    public DateTimeOffset CreatedAt { get; init; }

    public UserModel? CreatedBy { get; init; }

    public DateTimeOffset? ModifiedAt { get; init; }

    public UserModel? ModifiedBy { get; init; }
}
