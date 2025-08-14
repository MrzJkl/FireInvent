using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FireInvent.Api.Swagger;

public class AddResponseHeadersFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });

        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });
    }
}
