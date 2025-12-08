namespace FireInvent.Database;

/// <summary>
/// Scoped service that holds the current tenant context for the duration of a request.
/// This is populated by the TenantResolutionMiddleware and used by the AppDbContext
/// to filter queries and assign TenantId to new entities.
/// </summary>
public class TenantProvider
{
    /// <summary>
    /// The ID of the current tenant for this request.
    /// Null if no tenant has been resolved yet.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Indicates whether a tenant has been resolved for this request.
    /// </summary>
    public bool HasTenant => !string.IsNullOrEmpty(TenantId);
}
