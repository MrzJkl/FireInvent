using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using FireInvent.Shared.Options;
using Microsoft.Extensions.Options;
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
                        var authOptions = context.ApplicationServices.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
                        var openApiOptions = context.ApplicationServices.GetRequiredService<IOptions<OpenApiOptions>>().Value;

                        document.Info = new OpenApiInfo
                        {
                            Title = "FireInvent - Open Inventory Management",
                            Version = description?.ApiVersion.ToString() ?? context.DocumentName,
                            Description = description?.IsDeprecated == true ? "This API version is deprecated." : null
                        };

                        document.Components ??= new OpenApiComponents();
                        document.Components.Headers ??= new Dictionary<string, IOpenApiHeader>();
                        document.Components.Headers.Add("X-Resolved-Tenant-ID", new OpenApiHeader()
                        {
                            Description = "The resolved Tenant-ID from JWT. Clarifies the Tenant-Context of the response.",
                            Required = true,
                            AllowEmptyValue = false,
                        });

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

                        if (openApiOptions.AuthorizationUrl is not null && openApiOptions.TokenUrl is not null)
                        {
                            securitySchemes["OAuth2"] = new OpenApiSecurityScheme
                            {
                                Type = SecuritySchemeType.OAuth2,
                                Flows = new OpenApiOAuthFlows
                                {
                                    AuthorizationCode = new OpenApiOAuthFlow
                                    {
                                        AuthorizationUrl = new Uri(openApiOptions.AuthorizationUrl),
                                        TokenUrl = new Uri(openApiOptions.TokenUrl),
                                    }
                                }
                            };
                        }

                        // Apply it as a requirement for all operations
                        foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations ?? []))
                        {
                            operation.Value.Security ??= [];

                            if (securitySchemes.ContainsKey("OAuth2"))
                            {
                                operation.Value.Security.Add(new OpenApiSecurityRequirement
                                {
                                    [new OpenApiSecuritySchemeReference("OAuth2", document)] = []
                                });
                            }

                            operation.Value.Security.Add(new OpenApiSecurityRequirement
                            {
                                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                            });

                            operation.Value.Parameters ??= [];
                            operation.Value.Parameters.Add(new OpenApiParameter
                            {
                                Description = "Tenant-ID (Required if multiple organizations included in JWT). To be able to use Tenant-Admin-API set Guid.Empty as Tenant-ID.",
                                Name = "X-Tenant-ID",
                                Required = false,
                                In = ParameterLocation.Header,
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
            app.MapOpenApi().CacheOutput();

            app.MapScalarApiReference("/docs", (options, context) =>
            {
                var authOptions = context.RequestServices.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
                var openApiOptions = context.RequestServices.GetRequiredService<IOptions<OpenApiOptions>>().Value;

                options
                    .ExpandAllTags()
                    .ExpandAllModelSections()
                    .WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Axios)
                    .DisableTelemetry()
                    .AddPreferredSecuritySchemes("OAuth2");

                if (openApiOptions.AuthorizationUrl is not null &&  openApiOptions.TokenUrl is not null && openApiOptions.ApiExplorerClientId is not null)
                {
                    options.AddOAuth2Flows("OAuth2", flows =>
                        {
                            flows.AuthorizationCode = new AuthorizationCodeFlow
                            {
                                ClientId = openApiOptions.ApiExplorerClientId,
                                AuthorizationUrl = openApiOptions.AuthorizationUrl,
                                TokenUrl = openApiOptions.TokenUrl,
                                Pkce = Pkce.Sha256,
                            };
                        });
                }

            }).CacheOutput();

            return app;
        }
    }
}
