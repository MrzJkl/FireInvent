using FireInvent.Contract.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FireInvent.Shared.Extensions;

/// <summary>
/// Extension methods for handling deletions with Restrict behavior enforcement.
/// When a DbUpdateException occurs due to foreign key constraint violations,
/// it's converted to a DeleteFailureException with a user-friendly message.
/// </summary>
public static class DeleteOperationExtensions
{
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
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("FOREIGN KEY") == true)
        {
            throw new DeleteFailureException(
                $"Cannot delete {entityName} with ID {entityId} because other entities depend on it. " +
                $"Please remove or reassign all related data first.");
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
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("FOREIGN KEY") == true)
        {
            throw new DeleteFailureException(
                $"Cannot delete {entityName} with ID {entityId} because other entities depend on it. " +
                $"Please remove or reassign all related data first.");
        }
    }
}
