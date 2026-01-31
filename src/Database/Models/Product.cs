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

    public Guid? CreatedById { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    public Guid? ModifiedById { get; set; }

    [Required]
    public virtual ProductType Type { get; set; } = null!;

    [Required]
    public virtual Manufacturer Manufacturer { get; set; } = null!;

    public virtual ICollection<Variant> Variants { get; set; } = [];

    [Required]
    public virtual Tenant Tenant { get; set; } = null!;
    
    public virtual User? CreatedBy { get; set; }
    
    public virtual User? ModifiedBy { get; set; }
}
