namespace FireInvent.Contract.Exceptions;

public class BadRequestException(string? message = null) : Exception(message ?? "The request is invalid.")
{
}