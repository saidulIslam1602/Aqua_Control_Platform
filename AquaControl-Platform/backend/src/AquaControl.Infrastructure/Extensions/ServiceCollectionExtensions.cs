using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AquaControl.Application.Common.Interfaces;
using AquaControl.Infrastructure.Persistence;
using AquaControl.Infrastructure.ReadModels;
using AquaControl.Infrastructure.EventStore;
using AquaControl.Infrastructure.TimeSeries;

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

        // Time-Series Database (TimescaleDB)
        services.AddDbContext<TimeSeriesDbContext>(options =>
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

