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

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

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

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddProcessAllocatedMemoryHealthCheck(2000)
    .AddDiskStorageHealthCheck(null)
    .AddCheck("self", () => HealthCheckResult.Healthy());

builder.Services.Configure<AuthenticationOptions>(
    builder.Configuration.GetRequiredSection("Authentication"));
builder.Services.Configure<DefaultAdminOptions>(
    builder.Configuration.GetSection("DefaultUser"));
builder.Services.Configure<MailOptions>(
    builder.Configuration.GetSection("MailOptions"));

var authOptions = builder.Configuration.GetRequiredSection("Authentication").Get<AuthenticationOptions>()!;

builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = authOptions.Authority;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), x => x.MigrationsAssembly("FireInvent.Database")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.EnableAnnotations();

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FireInvent",
        Version = "v1",
        Description = "Manage your inventory a modern way!"
    });

    c.OperationFilter<AddResponseHeadersFilter>();

    c.AddSecurityDefinition("oidc", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OpenIdConnect,
        OpenIdConnectUrl = new Uri("https://auth.example.com/application/o/my-api/.well-known/openid-configuration"),
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

builder.Services.AddAutoMapper((config) => config.AddProfile<MappingProfile>());

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

app.MapHealthChecks("/health");

using var scope = app.Services.CreateScope();

var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
var log = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
await dbContext.Database.EnsureCreatedAsync();
var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
if (pendingMigrations.Any())
{
    log.LogInformation("Waiting for pending database migrations (${Migrations})...", pendingMigrations);
    try
    {
        await dbContext.Database.MigrateAsync();
        log.LogInformation("Successfully applied pending database migrations.");
    }
    catch (Exception ex)
    {
        log.LogCritical(ex, "Failed to apply pending database migrations.");
    }
}
else
{
    log.LogInformation("No pending database migrations.");
}

app.UseMiddleware<ApiExceptionMiddleware>();

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Meine API v1");
    c.OAuthClientId(authOptions.ClientIdForSwagger);
    c.OAuthScopes([.. authOptions.Scopes]);
    c.OAuthAppName("FireInvent Swagger");
});

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});


app.Run();