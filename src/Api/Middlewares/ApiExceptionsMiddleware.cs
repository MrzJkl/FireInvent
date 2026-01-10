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

        switch (exception)
        {
            case NotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = "Resource not found.";
                break;
            case ConflictException:
                statusCode = HttpStatusCode.Conflict;
                message = "Conflict occurred. This may indicate that you try to create or edit a resource with an unique identifier, that already exists.";
                break;
            case BadRequestException:
                statusCode = HttpStatusCode.BadRequest;
                message = "Bad request. Please check your input and try again.";
                break;
            case OperationCanceledException:
                statusCode = HttpStatusCode.RequestTimeout;
                message = "The operation was canceled.";
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = JsonSerializer.Serialize(new { message = message, traceIdentifier = context.TraceIdentifier });
        return context.Response.WriteAsync(response);
    }
}
