﻿using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

[Index(nameof(Name), nameof(ManufacturerId), IsUnique = true)]
[Index(nameof(ExternalIdentifier), nameof(ManufacturerId), IsUnique = true)]
public record Product : IHasTenant, IAuditable
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Tenant))]
    public Guid TenantId { get; set; }

    [ForeignKey(nameof(Type))]
    [Required]
    public Guid TypeId { get; set; }

    [Required]
    [ForeignKey(nameof(Manufacturer))]
    public Guid ManufacturerId { get; set; }

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Description { get; set; }

    [MaxLength(ModelConstants.MaxStringLength)]
    public string? ExternalIdentifier { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public Guid? CreatedById { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    [ForeignKey(nameof(ModifiedBy))]
    public Guid? ModifiedById { get; set; }

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual ProductType Type { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Restrict)]
    public virtual Manufacturer Manufacturer { get; set; } = null!;

    public virtual ICollection<Variant> Variants { get; set; } = [];

    [DeleteBehavior(DeleteBehavior.Cascade)]
    public virtual Tenant Tenant { get; set; } = null!;
    
    [DeleteBehavior(DeleteBehavior.SetNull)]
    public virtual User? CreatedBy { get; set; }
    
    [DeleteBehavior(DeleteBehavior.SetNull)]
    public virtual User? ModifiedBy { get; set; }
}
