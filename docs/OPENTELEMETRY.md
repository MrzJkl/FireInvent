# OpenTelemetry Integration Documentation

FireInvent API is instrumented with OpenTelemetry to provide comprehensive observability through distributed tracing, metrics collection, and structured logging.

## Overview

OpenTelemetry is an open-source observability framework that provides vendor-neutral APIs and tools for collecting telemetry data. The FireInvent API implementation follows .NET 10 best practices and includes extensive automatic and custom instrumentation.

## Features

### Automatic Instrumentation

The API automatically collects telemetry from:

1. **ASP.NET Core**
   - HTTP request/response tracing
   - HTTP metrics (request duration, status codes, etc.)
   - User-Agent and custom header tracking
   - Tenant ID propagation
   - Exception recording

2. **HttpClient**
   - Outbound HTTP calls (e.g., Keycloak API interactions)
   - Request/response tracking
   - Status code and URI recording

3. **Entity Framework Core**
   - Database query tracing
   - SQL statement recording
   - Query performance metrics

4. **Runtime Metrics**
   - Garbage Collection metrics
   - ThreadPool statistics
   - JIT compilation metrics
   - Memory usage

### Custom Business Metrics

The API includes custom metrics for domain-specific operations:

| Metric Name | Type | Description |
|------------|------|-------------|
| `fireinvent.items.created` | Counter | Total number of inventory items created |
| `fireinvent.items.assigned` | Counter | Total number of item assignments |
| `fireinvent.maintenance.records` | Counter | Total maintenance records created |
| `fireinvent.orders.created` | Counter | Total orders created |
| `fireinvent.visits.created` | Counter | Total visits created |
| `fireinvent.items.active` | Gauge | Current number of active items |
| `fireinvent.orders.pending` | Gauge | Current number of pending orders |
| `fireinvent.database.query.duration` | Histogram | Database query execution time |
| `fireinvent.orders.items` | Histogram | Number of items per order |
| `fireinvent.visits.items` | Histogram | Number of items per visit |

### Custom Activity Tracing

Business operations are traced with custom activities:

- `ItemService.CreateItem` - Item creation operations
- `MaintenanceService.CreateMaintenance` - Maintenance record creation
- `OrderService.CreateOrder` - Order creation
- `OrderService.GetAllOrders` - Order queries
- `ItemAssignmentHistoryService.CreateAssignment` - Item assignments

Each activity includes relevant tags for filtering and analysis (item IDs, variant IDs, user IDs, etc.).

## Configuration

### appsettings.json

Add OpenTelemetry configuration to your `appsettings.json`:

```json
{
  "OpenTelemetry": {
    "ServiceName": "FireInvent.Api",
    "OtlpEndpoint": ""
  }
}
```

**Configuration Options:**

- `ServiceName` (optional): Override the default service name. Default: "FireInvent.Api"
- `OtlpEndpoint` (optional): OTLP gRPC endpoint URL for exporting telemetry. Example: "http://localhost:4317"

### Environment Variables

You can also configure OpenTelemetry using environment variables:

```bash
# Service name
OpenTelemetry__ServiceName=FireInvent.Api

# OTLP endpoint
OpenTelemetry__OtlpEndpoint=http://otel-collector:4317
```

### Docker Compose Example

```yaml
version: '3.8'
services:
  api:
    image: fireinvent-api:latest
    environment:
      - OpenTelemetry__ServiceName=FireInvent.Api
      - OpenTelemetry__OtlpEndpoint=http://otel-collector:4317
    depends_on:
      - otel-collector

  otel-collector:
    image: otel/opentelemetry-collector-contrib:latest
    ports:
      - "4317:4317"  # OTLP gRPC
      - "4318:4318"  # OTLP HTTP
    volumes:
      - ./otel-collector-config.yaml:/etc/otel-collector-config.yaml
    command: ["--config=/etc/otel-collector-config.yaml"]
```

## Exporters

### Console Exporter (Development)

The console exporter is automatically enabled and outputs telemetry to the console. This is useful for local development and debugging.

**Example output:**
```
Activity.Id:          00-ab1234567890def-1234567890abcdef-01
Activity.DisplayName: ItemService.CreateItem
Activity.Kind:        Internal
Activity.StartTime:   2024-01-10T10:30:00.0000000Z
Activity.Duration:    00:00:00.1234567
Activity.Tags:
    variant.id: a1b2c3d4-e5f6-7890-abcd-ef1234567890
    item.identifier: ITEM-001
    item.id: d4e5f6a7-b8c9-0123-4567-890abcdef123
```

### OTLP Exporter (Production)

The OTLP (OpenTelemetry Protocol) exporter sends telemetry to an OpenTelemetry Collector or directly to backends like Jaeger, Tempo, or cloud observability platforms.

To enable OTLP export, set the `OtlpEndpoint` configuration:

```json
{
  "OpenTelemetry": {
    "OtlpEndpoint": "http://otel-collector:4317"
  }
}
```

## Integration with Observability Backends

### Jaeger (Distributed Tracing)

1. Deploy Jaeger with OTLP support:
```bash
docker run -d --name jaeger \
  -e COLLECTOR_OTLP_ENABLED=true \
  -p 4317:4317 \
  -p 16686:16686 \
  jaegertracing/all-in-one:latest
```

2. Configure API:
```json
{
  "OpenTelemetry": {
    "OtlpEndpoint": "http://jaeger:4317"
  }
}
```

3. Access Jaeger UI at `http://localhost:16686`

### Prometheus (Metrics)

1. Set up OpenTelemetry Collector with Prometheus exporter:

```yaml
# otel-collector-config.yaml
receivers:
  otlp:
    protocols:
      grpc:
        endpoint: 0.0.0.0:4317

exporters:
  prometheus:
    endpoint: "0.0.0.0:8889"

service:
  pipelines:
    metrics:
      receivers: [otlp]
      exporters: [prometheus]
```

2. Configure Prometheus to scrape the collector:

```yaml
# prometheus.yml
scrape_configs:
  - job_name: 'otel-collector'
    static_configs:
      - targets: ['otel-collector:8889']
```

### Grafana (Visualization)

Add data sources in Grafana:

1. **Tempo** for traces
   - Type: Tempo
   - URL: http://tempo:3200

2. **Prometheus** for metrics
   - Type: Prometheus
   - URL: http://prometheus:9090

3. **Loki** for logs (optional)
   - Type: Loki
   - URL: http://loki:3100

### Cloud Providers

#### Azure Monitor / Application Insights

```json
{
  "OpenTelemetry": {
    "OtlpEndpoint": "https://ingestion.azuremonitor.io/v1/traces"
  }
}
```

Set the `APPLICATIONINSIGHTS_CONNECTION_STRING` environment variable.

#### Google Cloud Trace

```json
{
  "OpenTelemetry": {
    "OtlpEndpoint": "https://cloudtrace.googleapis.com/v2/projects/YOUR_PROJECT_ID/traces"
  }
}
```

#### AWS X-Ray

Use AWS Distro for OpenTelemetry (ADOT) Collector:

```json
{
  "OpenTelemetry": {
    "OtlpEndpoint": "http://adot-collector:4317"
  }
}
```

## Resource Attributes

The following resource attributes are automatically added to all telemetry:

| Attribute | Description | Example |
|-----------|-------------|---------|
| `service.name` | Service identifier | FireInvent.Api |
| `service.version` | API version | 1.0.0 |
| `service.instance.id` | Instance/machine name | server-01 |
| `deployment.environment` | Environment name | Production |
| `host.name` | Host machine name | server-01 |
| `process.runtime.name` | Runtime name | .NET |
| `process.runtime.version` | Runtime version | 10.0.0 |

## Filtering

### Excluding Health Checks

Health check endpoints (`/health`) are automatically excluded from tracing to reduce noise.

## Correlation

### Trace Context Propagation

OpenTelemetry automatically propagates trace context using W3C Trace Context headers:
- `traceparent`: Contains trace ID, span ID, and trace flags
- `tracestate`: Contains vendor-specific trace information

This enables distributed tracing across services.

### Tenant Correlation

Custom tenant ID headers (`X-Tenant-Id`) are automatically captured and added as tags to traces, enabling tenant-specific filtering and analysis.

## Best Practices

### 1. Activity Naming

Use descriptive names that follow the pattern: `ServiceName.OperationName`

```csharp
using var activity = telemetry.StartActivity("ItemService.CreateItem");
```

### 2. Tag Important Data

Add relevant tags to activities for better filtering:

```csharp
activity?.SetTag("item.id", itemId);
activity?.SetTag("tenant.id", tenantId);
```

### 3. Record Business Metrics

Increment counters for important business events:

```csharp
telemetry.ItemsCreatedCounter.Add(1, 
    new KeyValuePair<string, object?>("variant.id", variantId));
```

### 4. Use Histogram for Durations

Track duration distributions using histograms:

```csharp
var stopwatch = Stopwatch.StartNew();
// ... perform operation ...
telemetry.DatabaseQueryDuration.Record(stopwatch.Elapsed.TotalMilliseconds);
```

### 5. Dispose Activities Properly

Always use `using` statement for activities to ensure proper disposal:

```csharp
using var activity = telemetry.StartActivity("MyOperation");
try
{
    // ... operation code ...
}
catch (Exception ex)
{
    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
    throw;
}
```

## Performance Considerations

1. **Sampling**: In high-volume scenarios, consider implementing sampling to reduce data volume
2. **Batch Export**: OTLP exporter batches data to reduce network overhead
3. **Async Export**: Telemetry export is asynchronous and doesn't block request processing
4. **Health Check Filtering**: Health checks are filtered to prevent excessive trace generation

## Troubleshooting

### No Telemetry Data

1. Check if `OtlpEndpoint` is configured correctly
2. Verify network connectivity to the OTLP endpoint
3. Check console output for telemetry data (console exporter is always enabled)
4. Verify OpenTelemetry Collector is running and accepting data

### Missing Traces

1. Ensure activities are created with `telemetry.StartActivity()`
2. Verify the activity source is registered in `OpenTelemetryExtensions`
3. Check sampling configuration if using sampling

### High Memory Usage

1. Review batch export settings
2. Consider implementing sampling for high-volume operations
3. Monitor the number of active activities

## Examples

### Adding Telemetry to a New Service

```csharp
using FireInvent.Shared.Services.Telemetry;
using System.Diagnostics;

public class MyService(AppDbContext context, FireInventTelemetry telemetry) : IMyService
{
    public async Task<MyModel> CreateAsync(CreateMyModel model, CancellationToken cancellationToken)
    {
        // Start a new activity for tracing
        using var activity = telemetry.StartActivity("MyService.Create");
        activity?.SetTag("my.property", model.Property);

        // Your business logic here
        var entity = new MyEntity { /* ... */ };
        await context.MyEntities.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // Record custom metrics
        telemetry.MyCustomCounter.Add(1,
            new KeyValuePair<string, object?>("property", model.Property));

        activity?.SetTag("entity.id", entity.Id);
        
        return MapToModel(entity);
    }
}
```

### Querying Traces in Jaeger

Filter traces by service: `service.name=FireInvent.Api`
Filter by operation: `operation=ItemService.CreateItem`
Filter by tag: `item.id=<guid>`

### Creating Grafana Dashboards

Use the included custom metrics for visualization:

```promql
# Rate of items created per second
rate(fireinvent_items_created_total[5m])

# Current active items
fireinvent_items_active

# Average database query duration
histogram_quantile(0.95, rate(fireinvent_database_query_duration_bucket[5m]))
```

## Further Reading

- [OpenTelemetry .NET Documentation](https://opentelemetry.io/docs/languages/net/)
- [OpenTelemetry Specification](https://opentelemetry.io/docs/specs/otel/)
- [ASP.NET Core Instrumentation](https://github.com/open-telemetry/opentelemetry-dotnet-contrib/tree/main/src/OpenTelemetry.Instrumentation.AspNetCore)
- [Entity Framework Core Instrumentation](https://github.com/open-telemetry/opentelemetry-dotnet-contrib/tree/main/src/OpenTelemetry.Instrumentation.EntityFrameworkCore)
