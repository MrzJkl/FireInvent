﻿using FireInvent.Contract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Database.Models;

public record Order : IHasTenant, IAuditable
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Tenant))]
    public Guid TenantId { get; set; }

    [MaxLength(ModelConstants.MaxStringLength)]
    public string? OrderIdentifier { get; set; }

    [Required]
    public DateOnly OrderDate { get; set; }

    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Draft;

    public DateOnly? DeliveryDate { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public Guid? CreatedById { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    [ForeignKey(nameof(ModifiedBy))]
    public Guid? ModifiedById { get; set; }

    public virtual ICollection<OrderItem> Items { get; set; } = [];

    [DeleteBehavior(DeleteBehavior.Cascade)]
    public virtual Tenant Tenant { get; set; } = null!;
    
    [DeleteBehavior(DeleteBehavior.SetNull)]
    public virtual User? CreatedBy { get; set; }
    
    [DeleteBehavior(DeleteBehavior.SetNull)]
    public virtual User? ModifiedBy { get; set; }
}
