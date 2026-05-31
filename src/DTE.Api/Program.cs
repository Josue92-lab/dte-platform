using DTE.Application;
using DTE.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Dependency Injection Bootstrap
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseRouting();

// Health Check Endpoint (Required for Commit 0)
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

// Required for Testcontainers/WebApplicationFactory integration
public partial class Program { }
