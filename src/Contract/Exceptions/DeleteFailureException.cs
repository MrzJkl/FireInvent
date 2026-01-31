namespace FireInvent.Contract.Exceptions;

public class DeleteFailureException(string? message = null) : Exception(message ?? "Failed to delete the entity. Check for related dependencies.")
{
}