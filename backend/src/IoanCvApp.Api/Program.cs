using IoanCvApp.Application;
using IoanCvApp.Infrastructure;
using IoanCvApp.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope();
    await DevelopmentDataSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program;
