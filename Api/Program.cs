using FlameGuardLaundry.Database;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<GearDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), x => x.MigrationsAssembly("FlameGuardLaundry.Database")));

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.MapGet("/weatherforecast", (HttpContext httpContext) =>
{

})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();
