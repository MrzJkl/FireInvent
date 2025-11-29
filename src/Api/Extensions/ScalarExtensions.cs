using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace FireInvent.Api.Extensions
{
    internal static class ScalarExtensions
    {
        internal static IServiceCollection AddOpenApi(this IServiceCollection services, List<ApiVersion> apiVersions)
        {
            foreach (var version in apiVersions)
            {
                services.Configure<ScalarOptions>(options => options.AddDocument($"v{version.MajorVersion}", $"v{version.MajorVersion}"));
                services.AddOpenApi($"v{version.MajorVersion}", options =>
                {
                    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
                    options.AddDocumentTransformer((document, context, ct) =>
                    {
                        var provider = context.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
                        var description = provider.ApiVersionDescriptions.FirstOrDefault(d => d.GroupName == context.DocumentName);

                        document.Info = new OpenApiInfo
                        {
                            Title = "FireInvent - Open Inventory Management",
                            Version = description?.ApiVersion.ToString() ?? context.DocumentName,
                            Description = description?.IsDeprecated == true ? "This API version is deprecated." : null
                        };

                        var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
                        {
                            ["Bearer"] = new OpenApiSecurityScheme
                            {
                                Type = SecuritySchemeType.Http,
                                Scheme = "bearer",
                                In = ParameterLocation.Header,
                                BearerFormat = "Json Web Token"
                            }
                        };

                        // Apply it as a requirement for all operations
                        foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
                        {
                            operation.Value.Security ??= [];
                            operation.Value.Security.Add(new OpenApiSecurityRequirement
                            {
                                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                            });
                        }
                        document.Components ??= new OpenApiComponents();
                        document.Components.SecuritySchemes = securitySchemes;

                        return Task.CompletedTask;
                    });
                });
            }

            return services;
        }

        internal static IApplicationBuilder ConfigureAddScalar(this WebApplication app)
        {
            app.MapOpenApi();

            app.MapScalarApiReference("/docs", (options, context) =>
            {
                options
                    .ExpandAllTags()
                    .ExpandAllModelSections()
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                    .AddPreferredSecuritySchemes("OAuth2", "Bearer");
            });

            return app;
        }
    }
}
