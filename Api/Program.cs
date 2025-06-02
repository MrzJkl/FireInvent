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

builder.Services.AddControllers();

WebApplication app = builder.Build();

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.Run();
