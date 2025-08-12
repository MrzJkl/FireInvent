using FireInvent.Api.Middlewares;
using FireInvent.Api.Swagger;
using FireInvent.Database;
using FireInvent.Shared;
using FireInvent.Shared.Options;
using FireInvent.Shared.Services;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
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

// Authentication & Authorization
builder.Services
    .AddAuthentication(AuthScheme)
    .AddJwtBearer(AuthScheme, options =>
    {
        options.Authority = authOptions.Authority;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false
        };

        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(context.Exception, "Authentication failed.");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

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
        OpenIdConnectUrl = new Uri(authOptions.OidcDiscoveryUrl),
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
            new[] { "openid", "profile", "email" } // scopes, die benötigt werden
        }
    });
});

// AutoMapper
builder.Services.AddAutoMapper(config => config.AddProfile<MappingProfile>());

// Application Services
builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<StorageLocationService>();
builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<ClothingProductService>();
builder.Services.AddScoped<ClothingVariantService>();
builder.Services.AddScoped<ClothingItemService>();
builder.Services.AddScoped<MaintenanceService>();
builder.Services.AddScoped<ClothingItemAssignmentHistoryService>();
builder.Services.AddScoped<UserService>();
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

// Health Endpoint
app.MapHealthChecks("/health");

var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Database migration & creation on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        logger.LogInformation("Ensuring datatbase is created.");
        await dbContext.Database.EnsureCreatedAsync();
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Failed ensure that database is created.");
        throw;
    }


    var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
    if (pendingMigrations.Any())
    {
        logger.LogInformation("Waiting for pending database migrations ({Migrations})...", pendingMigrations);
        try
        {
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Successfully applied pending database migrations.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to apply pending database migrations.");
        }
    }
    else
    {
        logger.LogInformation("No pending database migrations.");
    }
}

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

logger.LogDebug("Registering controllers end endpoints...");
app.MapControllers();

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

logger.LogInformation("Starting FireInvent API...");

app.Run();

logger.LogInformation("FireInvent API shutting down...");