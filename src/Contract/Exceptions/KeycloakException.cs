namespace FireInvent.Contract.Exceptions;

public class KeycloakException(string? message = null) : Exception(message ?? "Error while communicating with identity provider.")
{
}