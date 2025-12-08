using FireInvent.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FireInvent.Database.Models;

/// <summary>
/// Represents a tenant (e.g., a fire department) in the system.
/// Each tenant has isolated data through the TenantId discriminator.
/// </summary>
[Index(nameof(Realm), IsUnique = true)]
[Index(nameof(Name))]
public record Tenant
{
    /// <summary>
    /// Unique identifier for the tenant.
    /// </summary>
    [Key]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The Keycloak realm name for this tenant.
    /// Used to identify the tenant from JWT tokens.
    /// </summary>
    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Realm { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the tenant (e.g., "Fire Department Berlin").
    /// </summary>
    [Required]
    [MaxLength(ModelConstants.MaxStringLength)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description or additional information about the tenant.
    /// </summary>
    [MaxLength(ModelConstants.MaxStringLengthLong)]
    public string? Description { get; set; }

    /// <summary>
    /// Timestamp when the tenant was created.
    /// </summary>
    [Required]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Indicates whether the tenant is active.
    /// Inactive tenants cannot access the system.
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;
}
