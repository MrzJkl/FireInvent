﻿using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

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

    [ForeignKey(nameof(CreatedBy))]
    public Guid? CreatedById { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    [ForeignKey(nameof(ModifiedBy))]
    public Guid? ModifiedById { get; set; }

    [DeleteBehavior(DeleteBehavior.Cascade)]
    public virtual Order Order { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Cascade)]
    public virtual Variant Variant { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.Cascade)]
    public virtual Tenant Tenant { get; set; } = null!;

    [DeleteBehavior(DeleteBehavior.SetNull)]
    public virtual Person? Person { get; set; }
    
    [DeleteBehavior(DeleteBehavior.SetNull)]
    public virtual User? CreatedBy { get; set; }
    
    [DeleteBehavior(DeleteBehavior.SetNull)]
    public virtual User? ModifiedBy { get; set; }
}
