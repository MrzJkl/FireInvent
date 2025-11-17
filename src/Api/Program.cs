using FireInvent.Api.Authentication;
using FireInvent.Api.Middlewares;
using FireInvent.Api.Swagger;
using FireInvent.Contract;
using FireInvent.Database;
using FireInvent.Shared.Mapper;
using FireInvent.Shared.Options;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Serialization;

const string SwaggerApiVersion = "v1";
const string SwaggerEndpointUrl = $"/swagger/{SwaggerApiVersion}/swagger.json";
const string SwaggerApiTitle = "FireInvent";
const string SwaggerApiDescription = "Manage your inventory a modern way!";
const string AuthScheme = "Bearer";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configuration setup
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
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
builder.Services.Configure<MailOptions>(
    builder.Configuration.GetSection("MailOptions"));

var authOptions = builder.Configuration.GetRequiredSection("Authentication").Get<AuthenticationOptions>()!;

builder.Services.AddCustomAuthentication(AuthScheme, authOptions);
builder.Services.AddAuthorization();
builder.Services.AddMemoryCache();
builder.Services.AddResponseCompression();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsAssembly("FireInvent.Database")).UseLazyLoadingProxies());

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddProcessAllocatedMemoryHealthCheck(2000)
    .AddCheck("self", () => HealthCheckResult.Healthy());

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SchemaGeneratorOptions = new SchemaGeneratorOptions
    {
        UseInlineDefinitionsForEnums = true
    };

    c.MapType<ItemCondition>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = [.. Enum.GetNames<ItemCondition>()
            .Select(n => new OpenApiString(n))
            .Cast<IOpenApiAny>()]
    });

    c.MapType<OrderStatus>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = [.. Enum.GetNames<OrderStatus>()
            .Select(n => new OpenApiString(n))
            .Cast<IOpenApiAny>()]
    });

    c.EnableAnnotations();
    c.SwaggerDoc(SwaggerApiVersion, new OpenApiInfo
    {
        Title = SwaggerApiTitle,
        Version = SwaggerApiVersion,
        Description = SwaggerApiDescription
    });

    c.OperationFilter<AddResponseHeadersFilter>();

    c.AddSecurityDefinition("oidc", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OpenIdConnect,
        OpenIdConnectUrl = new Uri(authOptions.OidcDiscoveryUrlForSwagger),
        Description = "Login via OpenID Connect"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oidc"
                }
            },
            new[] { "openid", "profile", "email" }
        }
    });
});

#if DEBUG
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
#endif

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
builder.Services.AddTransient<MailService>();

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

logger.LogDebug("Registering swagger...");
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint(SwaggerEndpointUrl, $"{SwaggerApiTitle} {SwaggerApiVersion}");
    c.OAuthClientId(authOptions.ClientIdForSwagger);
    c.OAuthScopes([.. authOptions.Scopes]);
    c.OAuthAppName($"{SwaggerApiTitle} Swagger");
    c.OAuthUsePkce();
});

logger.LogDebug("Registering authentication and authorization...");
app.UseAuthentication();
app.UseAuthorization();
app.UseResponseCompression();

app.UseCors("AllowAll");

logger.LogDebug("Registering controllers end endpoints...");
app.MapControllers();

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
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