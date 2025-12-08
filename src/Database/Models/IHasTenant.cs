namespace FireInvent.Database.Models;

/// <summary>
/// Interface for entities that belong to a specific tenant.
/// Enables automatic tenant filtering and assignment.
/// </summary>
public interface IHasTenant
{
    /// <summary>
    /// The unique identifier of the tenant this entity belongs to.
    /// </summary>
    string TenantId { get; set; }
}
