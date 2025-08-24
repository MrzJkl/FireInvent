using FireInvent.Api.Authentication;
using FireInvent.Api.Middlewares;
using FireInvent.Api.Swagger;
using FireInvent.Database;
using FireInvent.Shared;
using FireInvent.Shared.Options;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Serilog;

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
builder.Services.Configure<DefaultAdminOptions>(
    builder.Configuration.GetSection("DefaultUser"));
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
        x => x.MigrationsAssembly("FireInvent.Database")));

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddProcessAllocatedMemoryHealthCheck(2000)
    .AddCheck("self", () => HealthCheckResult.Healthy());

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
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

// AutoMapper
builder.Services.AddAutoMapper(config => config.AddProfile<MappingProfile>());

// API Services
builder.Services.AddScoped<TokenValidatedHandler>();

// Shared Services
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IStorageLocationService, StorageLocationService>();
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IClothingProductService, ClothingProductService>();
builder.Services.AddScoped<IClothingVariantService, ClothingVariantService>();
builder.Services.AddScoped<IClothingItemService, ClothingItemService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<IClothingItemAssignmentHistoryService, ClothingItemAssignmentHistoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrderService, OrderService>();
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