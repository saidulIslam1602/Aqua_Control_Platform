using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AquaControl.Infrastructure.ReadModels;
using AquaControl.Infrastructure.EventStore;
using AquaControl.Infrastructure.TimeSeries;

namespace AquaControl.API.Extensions;

public static class WebApplicationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var readContext = scope.ServiceProvider.GetRequiredService<ReadModelDbContext>();
        var eventStoreContext = scope.ServiceProvider.GetRequiredService<EventStoreDbContext>();
        var timeSeriesContext = scope.ServiceProvider.GetRequiredService<TimeSeriesDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            // Ensure databases are created
            await readContext.Database.EnsureCreatedAsync();
            await eventStoreContext.Database.EnsureCreatedAsync();
            await timeSeriesContext.Database.EnsureCreatedAsync();
            
            // Initialize TimescaleDB
            await DatabaseInitializer.InitializeAsync(scope.ServiceProvider);
            
            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database initialization failed");
            throw;
        }
    }
}
