using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace AquaControl.Infrastructure.Services;

public class ExternalServiceHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExternalServiceHealthCheck> _logger;
    private readonly IConfiguration _configuration;

    public ExternalServiceHealthCheck(
        IHttpClientFactory httpClientFactory,
        ILogger<ExternalServiceHealthCheck> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var healthData = new Dictionary<string, object>();
        var issues = new List<string>();

        try
        {
            // Check external API dependencies
            await CheckExternalApi("weather-service", "https://api.openweathermap.org/data/2.5/weather", healthData, issues, cancellationToken);
            
            // Check other external services as needed
            // await CheckExternalApi("notification-service", "https://api.notification-service.com/health", healthData, issues, cancellationToken);

            if (issues.Any())
            {
                healthData["issues"] = issues;
                return HealthCheckResult.Degraded("Some external services are unavailable", data: healthData);
            }

            return HealthCheckResult.Healthy("All external services are available", healthData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External service health check failed");
            return HealthCheckResult.Unhealthy("External service health check failed", ex, healthData);
        }
    }

    private async Task CheckExternalApi(
        string serviceName,
        string endpoint,
        Dictionary<string, object> healthData,
        List<string> issues,
        CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);

            var response = await httpClient.GetAsync(endpoint, cancellationToken);
            
            healthData[$"{serviceName}_status"] = response.IsSuccessStatusCode ? "healthy" : "unhealthy";
            healthData[$"{serviceName}_response_time"] = $"{response.Headers.Date?.Subtract(DateTime.UtcNow).TotalMilliseconds ?? 0}ms";

            if (!response.IsSuccessStatusCode)
            {
                issues.Add($"{serviceName} returned {response.StatusCode}");
            }
        }
        catch (TaskCanceledException)
        {
            healthData[$"{serviceName}_status"] = "timeout";
            issues.Add($"{serviceName} timed out");
        }
        catch (Exception ex)
        {
            healthData[$"{serviceName}_status"] = "error";
            healthData[$"{serviceName}_error"] = ex.Message;
            issues.Add($"{serviceName} error: {ex.Message}");
        }
    }
}
