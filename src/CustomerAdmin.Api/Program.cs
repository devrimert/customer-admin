using CustomerAdmin.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Connection string 'Default' is not configured. See README for local setup.");

builder.Services.AddDbContext<CustomerAdminDbContext>(options =>
    options
        .UseNpgsql(connectionString)
        .UseSnakeCaseNamingConvention());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Serves the generated OpenAPI document at /openapi/v1.json
    app.MapOpenApi();

    // Interactive UI on top of the generated document, available at /swagger
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Customer Admin API v1");
    });
}

app.MapControllers();

app.Run();
