using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using FireInvent.Shared.Services.Telemetry;

namespace FireInvent.Api.Extensions;

public static class OpenTelemetryExtensions
{
    public const string ServiceName = FireInventTelemetry.ServiceName;

    public static IServiceCollection AddOpenTelemetryObservability(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var serviceName = configuration.GetValue<string>("OpenTelemetry:ServiceName") ?? ServiceName;
        var serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown";
        var serviceInstanceId = Environment.MachineName;

        services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource
                    .AddService(serviceName: serviceName, serviceVersion: serviceVersion, serviceInstanceId: serviceInstanceId)
                    .AddAttributes(new Dictionary<string, object>
                    {
                        ["deployment.environment"] = environment.EnvironmentName,
                        ["host.name"] = Environment.MachineName,
                        ["process.runtime.name"] = ".NET",
                        ["process.runtime.version"] = Environment.Version.ToString()
                    });
            })
            .WithTracing(tracing =>
            {
                tracing
                    // ASP.NET Core instrumentation - captures HTTP requests
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequest = (activity, httpRequest) =>
                        {
                            activity.SetTag("http.request.method", httpRequest.Method);
                            activity.SetTag("http.request.path", httpRequest.Path);
                            activity.SetTag("http.request.scheme", httpRequest.Scheme);
                            activity.SetTag("http.request.host", httpRequest.Host.Value);
                            
                            if (httpRequest.Headers.TryGetValue("User-Agent", out var userAgent))
                            {
                                activity.SetTag("http.user_agent", userAgent.ToString());
                            }
                            
                            if (httpRequest.Headers.TryGetValue("X-Tenant-Id", out var tenantId))
                            {
                                activity.SetTag("tenant.id", tenantId.ToString());
                            }
                        };
                        options.EnrichWithHttpResponse = (activity, httpResponse) =>
                        {
                            activity.SetTag("http.response.status_code", httpResponse.StatusCode);
                        };
                        options.Filter = httpContext =>
                        {
                            // Don't trace health checks to reduce noise
                            return !httpContext.Request.Path.StartsWithSegments("/health");
                        };
                    })
                    // HttpClient instrumentation - captures outgoing HTTP calls (e.g., to Keycloak)
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
                        {
                            activity.SetTag("http.request.method", httpRequestMessage.Method.ToString());
                            activity.SetTag("http.request.uri", httpRequestMessage.RequestUri?.ToString());
                        };
                        options.EnrichWithHttpResponseMessage = (activity, httpResponseMessage) =>
                        {
                            activity.SetTag("http.response.status_code", (int)httpResponseMessage.StatusCode);
                        };
                        options.FilterHttpRequestMessage = httpRequestMessage =>
                        {
                            // Trace all HTTP requests
                            return true;
                        };
                    })
                    // Entity Framework Core instrumentation - captures database operations
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                        options.SetDbStatementForStoredProcedure = true;
                        options.EnrichWithIDbCommand = (activity, command) =>
                        {
                            activity.SetTag("db.operation", command.CommandText);
                        };
                    })
                    // Custom application instrumentation
                    .AddSource(FireInventTelemetry.ActivitySource.Name)
                    // Add console exporter for development
                    .AddConsoleExporter(options =>
                    {
                        options.Targets = OpenTelemetry.Exporter.ConsoleExporterOutputTargets.Console;
                    });

                // Add OTLP exporter if configured
                var otlpEndpoint = configuration.GetValue<string>("OpenTelemetry:OtlpEndpoint");
                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    // ASP.NET Core metrics - HTTP server metrics
                    .AddAspNetCoreInstrumentation()
                    // HttpClient metrics - HTTP client metrics
                    .AddHttpClientInstrumentation()
                    // Runtime metrics - GC, ThreadPool, JIT, etc.
                    .AddRuntimeInstrumentation()
                    // Custom application metrics
                    .AddMeter(ServiceName)
                    // Add console exporter for development
                    .AddConsoleExporter(options =>
                    {
                        options.Targets = OpenTelemetry.Exporter.ConsoleExporterOutputTargets.Console;
                    });

                // Add OTLP exporter if configured
                var otlpEndpoint = configuration.GetValue<string>("OpenTelemetry:OtlpEndpoint");
                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    metrics.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
                }
            });

        return services;
    }
}
