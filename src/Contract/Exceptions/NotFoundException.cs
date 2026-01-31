namespace FireInvent.Contract.Exceptions;

public class NotFoundException(string? message = null) : Exception(message ?? "The requested entity was not found.")
{
}