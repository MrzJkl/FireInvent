using FlameGuardLaundry.Api.Middlewares;
using FlameGuardLaundry.Api.Swagger;
using FlameGuardLaundry.Contract;
using FlameGuardLaundry.Database;
using FlameGuardLaundry.Shared;
using FlameGuardLaundry.Shared.Options;
using FlameGuardLaundry.Shared.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetRequiredSection("Jwt"));
builder.Services.Configure<DefaultAdminOptions>(
    builder.Configuration.GetSection("DefaultUser"));

var jwtOptions = builder.Configuration.GetRequiredSection("Jwt").Get<JwtOptions>()!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutterWeb", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddAuthorization();

builder.Services.AddDbContext<GearDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), x => x.MigrationsAssembly("FlameGuardLaundry.Database")));

builder.Services.AddIdentityCore<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<GearDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.EnableAnnotations();
    c.OperationFilter<AddResponseHeadersFilter>();

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Insert your JWT-Token here."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<StorageLocationService>();
builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<ClothingProductService>();
builder.Services.AddScoped<ClothingVariantService>();
builder.Services.AddScoped<ClothingItemService>();
builder.Services.AddScoped<MaintenanceService>();
builder.Services.AddScoped<ClothingItemAssignmentHistoryService>();
builder.Services.AddScoped<UserService>();

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

app.UseMiddleware<ApiExceptionMiddleware>();

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFlutterWeb");

app.UseAuthentication();
app.UseAuthorization();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
var defaultUserOptions = services.GetRequiredService<IOptions<DefaultAdminOptions>>().Value;

try
{
    foreach (var roleName in Roles.AllRoles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    if (!string.IsNullOrWhiteSpace(defaultUserOptions.Email) &&
        !string.IsNullOrWhiteSpace(defaultUserOptions.Password))
    {
        var existingUser = await userManager.FindByEmailAsync(defaultUserOptions.Email);
        if (existingUser == null)
        {
            var adminUser = new IdentityUser
            {
                UserName = defaultUserOptions.Email,
                Email = defaultUserOptions.Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, defaultUserOptions.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                Console.WriteLine($"Default user created: {defaultUserOptions.Email}");
            }
            else
            {
                Console.WriteLine("Failed to create default admin user:");
                foreach (var error in result.Errors)
                    Console.WriteLine($"- {error.Description}");
            }
        }
        else
        {
            Console.WriteLine("Default user already exists.");
        }
    }
    else
    {
        Console.WriteLine("No default user created (DefaultUser section incomplete).");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error during role/user setup: {ex.Message}");
}


app.Run();