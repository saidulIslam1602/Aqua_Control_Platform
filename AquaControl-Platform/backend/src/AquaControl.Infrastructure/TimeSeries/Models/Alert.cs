namespace AquaControl.Infrastructure.TimeSeries.Models;

/// <summary>
/// Represents an alert stored in TimescaleDB for time-series analysis.
/// Alerts are generated when sensor readings exceed thresholds or anomalies are detected.
/// </summary>
public sealed class Alert
{
    public Guid Id { get; set; }
    public Guid TankId { get; set; }
    public Guid? SensorId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public decimal? ThresholdValue { get; set; }
    public decimal? ActualValue { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
}

