using Asp.Versioning;
using FireInvent.Api.Authentication;
using FireInvent.Api.Extensions;
using FireInvent.Api.Middlewares;
using FireInvent.Contract;
using FireInvent.Database;
using FireInvent.Shared.Converter;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Options;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Serilog;
using System.Text.Json.Serialization;

const string AuthScheme = "Bearer";
const string CorsPolicyName = "FireInventCors";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configuration setup
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
#if DEBUG
    .AddUserSecrets("fireinvent")
#endif
    .AddEnvironmentVariables();

// Serilog setup
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithClientIp()
        .Enrich.WithProcessId()
        .Enrich.WithThreadId()
        .Enrich.WithCorrelationId();
});

// Configure Options
builder.Services.Configure<AuthenticationOptions>(
    builder.Configuration.GetRequiredSection("Authentication"));
builder.Services.Configure<CorsOptions>(
    builder.Configuration.GetSection("Cors"));
builder.Services.Configure<KeycloakAdminOptions>(
    builder.Configuration.GetSection("KeycloakAdmin"));

var authOptions = builder.Configuration.GetRequiredSection("Authentication").Get<AuthenticationOptions>()!;
var corsOptions = builder.Configuration.GetSection("Cors").Get<CorsOptions>() ?? new CorsOptions();

builder.Services.AddCustomAuthentication(AuthScheme, authOptions);
builder.Services.AddAuthorization();
builder.Services.AddMemoryCache();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Multi-Tenancy
builder.Services.AddScoped<TenantProvider>();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsAssembly("FireInvent.Database")).UseLazyLoadingProxies());

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddProcessAllocatedMemoryHealthCheck(2000)
    .AddCheck("self", () => HealthCheckResult.Healthy());

// Scalar API-Explorer
builder.Services.AddVersioning();

var versions = new List<ApiVersion>
{
    new(1, 0)
};

builder.Services.AddOpenApi(versions);

// CORS configuration
if (corsOptions.Enabled)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(CorsPolicyName, policy =>
        {
            if (corsOptions.AllowedOrigins.Count == 0)
            {
                policy.AllowAnyOrigin();
            }
            else
            {
                policy.WithOrigins(corsOptions.AllowedOrigins.ToArray());
            }

            if (corsOptions.AllowedMethods.Count == 0)
            {
                policy.AllowAnyMethod();
            }
            else
            {
                policy.WithMethods(corsOptions.AllowedMethods.ToArray());
            }

            if (corsOptions.AllowedHeaders.Count == 0)
            {
                policy.AllowAnyHeader();
            }
            else
            {
                policy.WithHeaders(corsOptions.AllowedHeaders.ToArray());
            }

            if (corsOptions.AllowCredentials)
            {
                policy.AllowCredentials();
            }
        });
    });
}

// API Services
builder.Services.AddScoped<TokenValidatedHandler>();

// Mappers
builder.Services.AddSingleton<DepartmentMapper>();
builder.Services.AddSingleton<ItemAssignmentHistoryMapper>();
builder.Services.AddSingleton<ItemMapper>();
builder.Services.AddSingleton<MaintenanceMapper>();
builder.Services.AddSingleton<OrderMapper>();
builder.Services.AddSingleton<PersonMapper>();
builder.Services.AddSingleton<ProductMapper>();
builder.Services.AddSingleton<StorageLocationMapper>();
builder.Services.AddSingleton<UserMapper>();
builder.Services.AddSingleton<VariantMapper>();
builder.Services.AddSingleton<ProductTypeMapper>();
builder.Services.AddSingleton<MaintenanceTypeMapper>();

// Shared Services
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IStorageLocationService, StorageLocationService>();
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IVariantService, VariantService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<IItemAssignmentHistoryService, ItemAssignmentHistoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IMaintenanceTypeService, MaintenanceTypeService>();
builder.Services.AddScoped<IProductTypeService, ProductTypeService>();
builder.Services.AddHttpClient<IKeycloakAdminService, KeycloakAdminService>();

// Controllers
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter());
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.Converters.Add(new EmptyStringToNullConverter());
});

WebApplication app = builder.Build();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "Handled {RequestPath} {RequestMethod} responded {StatusCode} in {Elapsed:0.00} ms";

    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? string.Empty);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
    };
});

// Health Endpoint
app.MapHealthChecks("/health");

var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Middlewares & Endpoints
logger.LogDebug("Registering middlewares...");
app.UseMiddleware<ApiExceptionMiddleware>();

logger.LogDebug("Registering scalar...");
app.ConfigureAddScalar();

logger.LogDebug("Registering authentication and authorization...");
app.UseAuthentication();
app.UseAuthorization();

// Register tenant resolution middleware after authentication
app.UseMiddleware<TenantResolutionMiddleware>();

app.UseResponseCompression();

if (corsOptions.Enabled)
{
    app.UseCors(CorsPolicyName);
}

logger.LogDebug("Registering controllers end endpoints...");
app.MapControllers();

app.MapGet("/", context =>
{
    context.Response.Redirect("/scalar");
    return Task.CompletedTask;
});

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var maxRetries = 5;
    var delay = TimeSpan.FromSeconds(2);

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {Count} pending migrations: {Migrations}", pendingMigrations.Count(), string.Join(", ", pendingMigrations));
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Successfully applied pending database migrations.");
            }
            else
            {
                logger.LogInformation("No pending database migrations.");
            }

            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database not ready yet (Attempt {Attempt}/{MaxRetries}).", attempt, maxRetries);

            if (attempt == maxRetries)
            {
                logger.LogCritical(ex, "Initial Database creation or migration failed after {MaxRetries} attempts.", maxRetries);
                throw;
            }

            await Task.Delay(delay);
        }
    }
}

logger.LogInformation("Starting FireInvent API...");

app.Run();

logger.LogInformation("FireInvent API shutting down...");