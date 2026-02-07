using FireInvent.Contract.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FireInvent.Shared.Extensions;

/// <summary>
/// Extension methods for handling deletions with Restrict behavior enforcement.
/// When a DbUpdateException occurs due to foreign key constraint violations,
/// it's converted to a DeleteFailureException with a user-friendly message.
/// </summary>
public static class DeleteOperationExtensions
{
    /// <summary>
    /// Determines if a DbUpdateException is caused by a foreign key constraint violation.
    /// Checks for PostgreSQL-specific exception (SqlState 23503) for reliable detection.
    /// </summary>
    private static bool IsForeignKeyViolation(DbUpdateException exception)
    {
        // Check for PostgreSQL foreign key violation (SqlState 23503)
        if (exception.InnerException is PostgresException pgException)
        {
            return pgException.SqlState == "23503";
        }
        
        // Fallback: check for "FOREIGN KEY" in message for other providers
        // This is less reliable but provides compatibility with other database providers
        return exception.InnerException?.Message.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) == true;
    }

    public static async Task<bool> TryDeleteEntityAsync(
        this DbContext context,
        Guid entityId,
        string entityName,
        Func<Guid, CancellationToken, Task<object?>> getEntity,
        Action<object> deleteAction,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await getEntity(entityId, cancellationToken);
            if (entity is null)
                return false;

            deleteAction(entity);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) when (IsForeignKeyViolation(ex))
        {
            throw new DeleteFailureException(
                $"Cannot delete {entityName} with ID {entityId} because other entities depend on it. " +
                $"Please remove or reassign all related data first.", ex);
        }
    }
    
    public static async Task<bool> TryDeleteEntityAsync<TEntity>(
        this DbContext context,
        Guid entityId,
        string entityName,
        DbSet<TEntity> dbSet,
        CancellationToken cancellationToken = default) where TEntity : class
    {
        try
        {
            var entity = await dbSet.FindAsync(new object[] { entityId }, cancellationToken: cancellationToken);
            if (entity is null)
                return false;

            dbSet.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex) when (IsForeignKeyViolation(ex))
        {
            throw new DeleteFailureException(
                $"Cannot delete {entityName} with ID {entityId} because other entities depend on it. " +
                $"Please remove or reassign all related data first.", ex);
        }
    }
}
