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
        string message = exception.Message;

        switch (exception)
        {
            case NotFoundException:
                statusCode = HttpStatusCode.NotFound;
                break;
            case ConflictException:
                statusCode = HttpStatusCode.Conflict;
                break;
            case BadRequestException:
                statusCode = HttpStatusCode.BadRequest;
                break;
            case OperationCanceledException:
                statusCode = HttpStatusCode.RequestTimeout;
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = JsonSerializer.Serialize(new { error = message });
        return context.Response.WriteAsync(response);
    }
}
