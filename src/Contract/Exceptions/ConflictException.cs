namespace FireInvent.Contract.Exceptions;

public class ConflictException(string? message = null) : Exception(message ?? "A conflict occurred with the current state of the resource.")
{
}