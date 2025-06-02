using FlameGuardLaundry.Api.Middlewares;
using FlameGuardLaundry.Database;
using FlameGuardLaundry.Shared.Services;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GearDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), x => x.MigrationsAssembly("FlameGuardLaundry.Database")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<StorageLocationService>();
builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<ClothingProductService>();
builder.Services.AddScoped<ClothingVariantService>();
builder.Services.AddScoped<ClothingItemService>();
builder.Services.AddScoped<MaintenanceService>();
builder.Services.AddScoped<ClothingItemAssignmentHistoryService>();

builder.Services.AddControllers();

WebApplication app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.Run();
