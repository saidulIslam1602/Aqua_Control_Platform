using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AquaControl.Application.Common.Interfaces;
using AquaControl.Infrastructure.Persistence;
using AquaControl.Infrastructure.ReadModels;
using AquaControl.Infrastructure.EventStore;

namespace AquaControl.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database - Read Model
        services.AddDbContext<ReadModelDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is missing"),
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                    
                    npgsqlOptions.CommandTimeout(30);
                });

            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });

        // Event Store
        services.AddDbContext<EventStoreDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is missing"),
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                    
                    npgsqlOptions.CommandTimeout(30);
                });

            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });

        // Repositories
        services.AddScoped<IEventStore, EventStoreRepository>();
        services.AddScoped<ITankRepository, TankRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}

public static class WebApplicationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var readContext = scope.ServiceProvider.GetRequiredService<ReadModelDbContext>();
        var eventStoreContext = scope.ServiceProvider.GetRequiredService<EventStoreDbContext>();

        try
        {
            // Ensure databases are created
            await readContext.Database.EnsureCreatedAsync();
            await eventStoreContext.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while initializing the database");
        }
    }
}

