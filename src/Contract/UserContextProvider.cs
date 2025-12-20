namespace FireInvent.Contract;

/// <summary>
/// Scoped service that holds the current user context for the duration of a request.
/// This is populated by the UserContextResolutionMiddleware and used by the AppDbContext
/// to filter queries and assign TenantId to new entities.
/// </summary>
public class UserContextProvider
{
    /// <summary>
    /// Null if no tenant has been resolved yet.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Null if no user has been resolved yet.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Null if no tenant has been resolved yet.
    /// </summary>
    public string? Name { get; set; } = string.Empty;

    /// <summary>
    /// Null if no tenant has been resolved yet.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Null if no tenant has been resolved yet.
    /// </summary>
    public DateTimeOffset? CreatedAt { get; set; }
}
