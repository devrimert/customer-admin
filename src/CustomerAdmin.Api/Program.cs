using CustomerAdmin.Api.Data;
using Microsoft.EntityFrameworkCore;
using CustomerAdmin.Api.Common;
using CustomerAdmin.Api.Features.Customers;
using CustomerAdmin.Api.Features.Contacts;

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

builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<ContactService>();
// RFC 9457 problem details for all error responses (validation, 404, 409, unhandled exceptions)
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ConflictExceptionHandler>();

var app = builder.Build();

// Apply pending migrations on startup
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CustomerAdminDbContext>();
    await db.Database.MigrateAsync();
}

app.UseExceptionHandler();

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
