using FireInvent.Contract.Exceptions;
using System.Net;
using System.Text.Json;

namespace FireInvent.Api.Middlewares;

public class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            try
            {
                await HandleExceptionAsync(context, ex);
            }
            catch (Exception innerEx)
            {
                logger.LogError(innerEx, "Failed to handle exception in ApiExceptionMiddleware");
            }
        }
    }


    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
        string message = "Unexpected error occurred.";
        string? details = null;

        switch (exception)
        {
            case NotFoundException notFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = "Resource not found.";
                details = notFoundException.Message;
                break;
            case ConflictException conflictException:
                statusCode = HttpStatusCode.Conflict;
                message = "Conflict occurred. This may indicate that you try to create or edit a resource with an unique identifier, that already exists.";
                details = conflictException.Message;
                break;
            case BadRequestException badRequestException:
                statusCode = HttpStatusCode.BadRequest;
                message = "Bad request. Please check your input and try again.";
                details = badRequestException.Message;
                break;
            case OperationCanceledException:
                statusCode = HttpStatusCode.RequestTimeout;
                message = "The operation was canceled.";
                break;
            case DeleteFailureException deleteFailureException:
                statusCode = HttpStatusCode.Conflict;
                message = "Delete failed. Please check for related dependencies and try again.";
                details = deleteFailureException.Message;
                break;
            case KeycloakException:
                statusCode = HttpStatusCode.InternalServerError;
                message = "Error while communicating with identity provider.";
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = JsonSerializer.Serialize(new { message, traceIdentifier = context.TraceIdentifier, details });
        return context.Response.WriteAsync(response);
    }
}
