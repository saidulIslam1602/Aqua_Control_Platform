using AquaControl.API.Extensions;
using AquaControl.API.Hubs;
using AquaControl.API.Middleware;
using AquaControl.Application.Extensions;
using AquaControl.Infrastructure.Extensions;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/aquacontrol-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting AquaControl API");

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    
    // Add custom service extensions
    builder.Services.AddSwaggerDocumentation();
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddAuthenticationServices(builder.Configuration);
    builder.Services.AddCorsServices();
    builder.Services.AddHealthCheckServices(builder.Configuration);
    builder.Services.AddSignalR();
    builder.Services.AddSignalRServices();
    
    // Add GraphQL (commented out until GraphQL types are implemented)
    // builder.Services
    //     .AddGraphQLServer()
    //     .AddQueryType<Query>()
    //     .AddMutationType<Mutation>()
    //     .AddSubscriptionType<Subscription>()
    //     .AddFiltering()
    //     .AddSorting()
    //     .AddProjections();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "AquaControl API V1");
            c.RoutePrefix = string.Empty; // Serve Swagger UI at root
        });
    }

    // Add custom middleware
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    // app.MapGraphQL("/graphql");
    app.MapHub<TankDataHub>("/hubs/tankdata");
    
    // Health checks
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    // Database initialization
    await app.InitializeDatabaseAsync();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { } // For testing

