using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace AquaControl.Infrastructure.Services;

public class ApplicationHealthCheck : IHealthCheck
{
    private readonly ILogger<ApplicationHealthCheck> _logger;

    public ApplicationHealthCheck(ILogger<ApplicationHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check application-specific health indicators
            var healthData = new Dictionary<string, object>
            {
                ["status"] = "healthy",
                ["timestamp"] = DateTime.UtcNow,
                ["version"] = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown",
                ["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown",
                ["uptime"] = TimeSpan.FromMilliseconds(Environment.TickCount64).ToString(),
                ["memory_usage"] = $"{GC.GetTotalMemory(false) / 1024 / 1024} MB",
                ["gc_collections"] = new
                {
                    gen0 = GC.CollectionCount(0),
                    gen1 = GC.CollectionCount(1),
                    gen2 = GC.CollectionCount(2)
                }
            };

            // Check critical application components
            var criticalChecks = new List<string>();
            
            // Add your critical component checks here
            // Example: Check if required services are available
            
            if (criticalChecks.Any())
            {
                healthData["issues"] = criticalChecks;
                return Task.FromResult(HealthCheckResult.Degraded("Some non-critical issues detected", data: healthData));
            }

            return Task.FromResult(HealthCheckResult.Healthy("Application is healthy", healthData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("Application health check failed", ex));
        }
    }
}
