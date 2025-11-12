namespace AquaControl.Infrastructure.TimeSeries.Models;

/// <summary>
/// Represents a sensor reading stored in TimescaleDB for time-series analysis.
/// This is optimized for time-series queries and aggregations.
/// </summary>
public sealed class SensorReading
{
    public Guid Id { get; set; }
    public Guid SensorId { get; set; }
    public Guid TankId { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal Value { get; set; }
    public decimal QualityScore { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
}

