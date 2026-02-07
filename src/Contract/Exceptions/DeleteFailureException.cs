namespace FireInvent.Contract.Exceptions;

public class DeleteFailureException : Exception
{
    public DeleteFailureException(string? message = null) 
        : base(message ?? "Failed to delete the entity. Check for related dependencies.")
    {
    }

    public DeleteFailureException(string? message, Exception? innerException) 
        : base(message ?? "Failed to delete the entity. Check for related dependencies.", innerException)
    {
    }
}