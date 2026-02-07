﻿using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Database.Models;

public record Maintenance : IHasTenant, IAuditable
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Tenant))]
    public Guid TenantId { get; set; }

    [Required]
    [ForeignKey(nameof(Item))]
    public Guid ItemId { get; set; }

    [Required]
    [ForeignKey(nameof(Type))]
    public Guid TypeId { get; set; }

    [Required]
    public DateTimeOffset PerformedAt { get; set; }
    
    [ForeignKey(nameof(PerformedBy))]
    public Guid? PerformedById { get; set; }

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Remarks { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public Guid? CreatedById { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    [ForeignKey(nameof(ModifiedBy))]
    public Guid? ModifiedById { get; set; }
    
    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual MaintenanceType Type { get; set; } = null!;
    
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public virtual Item Item { get; set; } = null!;
    
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public virtual Tenant Tenant { get; set; } = null!;
    
    [DeleteBehavior(DeleteBehavior.SetNull)]
    public virtual User? CreatedBy { get; set; }
    
    [DeleteBehavior(DeleteBehavior.SetNull)]
    public virtual User? ModifiedBy { get; set; }
    
    [DeleteBehavior(DeleteBehavior.SetNull)]
    public virtual User? PerformedBy { get; set; }
}
