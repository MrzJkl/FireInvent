﻿using System.ComponentModel.DataAnnotations;

namespace FireInvent.Shared.Models;

public record MaintenanceModel : CreateOrUpdateMaintenanceModel
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    public MaintenanceTypeModel Type { get; init; } = null!;

    [Required]
    public DateTimeOffset CreatedAt { get; init; }

    public UserModel? CreatedBy { get; init; }

    public DateTimeOffset? ModifiedAt { get; init; }

    public UserModel? ModifiedBy { get; init; }
    
    public UserModel? PerformedBy { get; init; }
}
