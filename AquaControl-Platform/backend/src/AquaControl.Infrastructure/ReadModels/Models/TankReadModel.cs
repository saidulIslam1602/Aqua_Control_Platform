namespace AquaControl.Infrastructure.ReadModels.Models;

public sealed class TankReadModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Capacity { get; set; }
    public string CapacityUnit { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string? Zone { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string TankType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public long Version { get; set; }
}

public sealed class SensorReadModel
{
    public Guid Id { get; set; }
    public Guid TankId { get; set; }
    public string SensorType { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal Accuracy { get; set; }
    public DateTime InstallationDate { get; set; }
    public DateTime? CalibrationDate { get; set; }
    public DateTime? NextCalibrationDate { get; set; }
    public long Version { get; set; }
}

public sealed class SensorReadingReadModel
{
    public Guid Id { get; set; }
    public Guid SensorId { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal Value { get; set; }
    public decimal QualityScore { get; set; }
    public string? Metadata { get; set; }
}

