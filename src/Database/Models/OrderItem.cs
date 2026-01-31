﻿using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

public record OrderItem : IHasTenant, IAuditable
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Tenant))]
    public Guid TenantId { get; set; }

    [Required]
    [ForeignKey(nameof(Order))]
    public Guid OrderId { get; set; }

    [Required]
    [ForeignKey(nameof(Variant))]
    public Guid VariantId { get; set; }

    [ForeignKey(nameof(Person))]
    public Guid? PersonId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    public Guid? CreatedById { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    public Guid? ModifiedById { get; set; }

    [Required]
    public virtual Order Order { get; set; } = null!;

    [Required]
    public virtual Variant Variant { get; set; } = null!;

    [Required]
    public virtual Tenant Tenant { get; set; } = null!;

    public virtual Person? Person { get; set; }
    
    public virtual User? CreatedBy { get; set; }
    
    public virtual User? ModifiedBy { get; set; }
}
