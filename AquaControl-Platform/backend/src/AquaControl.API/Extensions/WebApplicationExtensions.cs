using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AquaControl.Infrastructure.ReadModels;
using AquaControl.Infrastructure.EventStore;
using AquaControl.Infrastructure.TimeSeries;
using AquaControl.Infrastructure.Persistence;
using AquaControl.Application.Services;

namespace AquaControl.API.Extensions;

public static class WebApplicationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var readContext = scope.ServiceProvider.GetRequiredService<ReadModelDbContext>();
        var eventStoreContext = scope.ServiceProvider.GetRequiredService<EventStoreDbContext>();
        var timeSeriesContext = scope.ServiceProvider.GetRequiredService<TimeSeriesDbContext>();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            // Ensure databases are created
            await applicationContext.Database.EnsureCreatedAsync();
            await readContext.Database.EnsureCreatedAsync();
            await eventStoreContext.Database.EnsureCreatedAsync();
            await timeSeriesContext.Database.EnsureCreatedAsync();
            
            // Initialize TimescaleDB
            logger.LogInformation("Initializing TimescaleDB...");
            await DatabaseInitializer.InitializeAsync(scope.ServiceProvider);
            logger.LogInformation("TimescaleDB initialization completed");
            
            // Create default admin user
            logger.LogInformation("Creating default admin user...");
            var adminUserResult = await authService.CreateDefaultAdminUserAsync();
            logger.LogInformation("Default admin user creation result: {Result}", adminUserResult);
            
            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database initialization failed");
            throw;
        }
    }
}
