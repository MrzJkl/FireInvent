using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireInvent.Database.Models;

[Index(nameof(ProductId), nameof(Name), IsUnique = true)]
[Index(nameof(Name))]
[Index(nameof(ExternalIdentifier), nameof(ProductId), IsUnique = true)]
public record Variant : IHasTenant, IAuditable
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Tenant))]
    public Guid TenantId { get; set; }

    [Required]
    [ForeignKey(nameof(Product))]
    public Guid ProductId { get; set; }

    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? AdditionalSpecs { get; set; }

    [MaxLength(ModelConstants.MaxStringLength)]
    public string? ExternalIdentifier { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    [Required]
    public Guid CreatedById { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }

    public Guid? ModifiedById { get; set; }

    [Required]
    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<Item> Items { get; set; } = [];

    [Required]
    public virtual Tenant Tenant { get; set; } = null!;
}
