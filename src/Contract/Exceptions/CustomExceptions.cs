namespace FireInvent.Contract.Exceptions;

public class ConflictException(string? message = null) : Exception(message ?? "A conflict occurred with the current state of the resource.")
{
}

public class NotFoundException(string? message = null) : Exception(message ?? "The requested entity was not found.")
{
}

public class BadRequestException(string? message = null) : Exception(message ?? "The request is invalid.")
{
}

public class KeycloakException(string? message = null) : Exception(message ?? "Error while communcating with identity provider.")
{
}
