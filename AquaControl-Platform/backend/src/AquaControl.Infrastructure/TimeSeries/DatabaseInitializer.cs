using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AquaControl.Infrastructure.TimeSeries;

/// <summary>
/// Initializes TimescaleDB database with extensions, hypertables, and policies.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Initializes the TimescaleDB database with extensions, schemas, and hypertables.
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TimeSeriesDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TimeSeriesDbContext>>();

        try
        {
            logger.LogInformation("Starting TimescaleDB initialization...");

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Enable TimescaleDB extension
            await EnableTimescaleDBAsync(context, logger);

            // Create schemas
            await CreateSchemasAsync(context, logger);

            // Create hypertables
            await CreateHypertablesAsync(context, logger);

            // Add compression and retention policies
            await AddPoliciesAsync(context, logger);

            logger.LogInformation("TimescaleDB initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing TimescaleDB");
            throw;
        }
    }

    private static async Task EnableTimescaleDBAsync(TimeSeriesDbContext context, ILogger logger)
    {
        try
        {
            logger.LogInformation("Enabling TimescaleDB extension...");

            await context.Database.ExecuteSqlRawAsync(
                "CREATE EXTENSION IF NOT EXISTS timescaledb CASCADE;");

            logger.LogInformation("TimescaleDB extension enabled successfully");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to enable TimescaleDB extension - continuing without it");
        }
    }

    private static async Task CreateSchemasAsync(TimeSeriesDbContext context, ILogger logger)
    {
        try
        {
            logger.LogInformation("Creating database schemas...");

            await context.Database.ExecuteSqlRawAsync(
                "CREATE SCHEMA IF NOT EXISTS timeseries;");

            logger.LogInformation("Database schemas created successfully");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to create schemas - continuing");
        }
    }

    private static async Task CreateHypertablesAsync(TimeSeriesDbContext context, ILogger logger)
    {
        try
        {
            logger.LogInformation("Creating TimescaleDB hypertables...");

            // Create hypertable for sensor readings
            await context.Database.ExecuteSqlRawAsync(@"
                SELECT create_hypertable('timeseries.""SensorReadings""', 'Timestamp', 
                    chunk_time_interval => INTERVAL '1 hour',
                    if_not_exists => TRUE);
            ");

            // Create hypertable for alerts
            await context.Database.ExecuteSqlRawAsync(@"
                SELECT create_hypertable('timeseries.""Alerts""', 'CreatedAt', 
                    chunk_time_interval => INTERVAL '1 day',
                    if_not_exists => TRUE);
            ");

            logger.LogInformation("TimescaleDB hypertables created successfully");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to create TimescaleDB hypertables - continuing without them");
        }
    }

    private static async Task AddPoliciesAsync(TimeSeriesDbContext context, ILogger logger)
    {
        try
        {
            logger.LogInformation("Adding TimescaleDB policies...");

            // Add compression policy for older data
            await context.Database.ExecuteSqlRawAsync(@"
                SELECT add_compression_policy('timeseries.""SensorReadings""', 
                    INTERVAL '7 days', 
                    if_not_exists => TRUE);
            ");

            await context.Database.ExecuteSqlRawAsync(@"
                SELECT add_compression_policy('timeseries.""Alerts""', 
                    INTERVAL '30 days', 
                    if_not_exists => TRUE);
            ");

            // Add retention policy
            await context.Database.ExecuteSqlRawAsync(@"
                SELECT add_retention_policy('timeseries.""SensorReadings""', 
                    INTERVAL '1 year', 
                    if_not_exists => TRUE);
            ");

            logger.LogInformation("TimescaleDB policies added successfully");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to add TimescaleDB policies - continuing without them");
        }
    }
}

