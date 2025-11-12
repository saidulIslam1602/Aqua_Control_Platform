namespace AquaControl.Application.Features.Tanks.DTOs;

public sealed class TankDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Capacity { get; init; }
    public string CapacityUnit { get; init; } = string.Empty;
    public LocationDto Location { get; init; } = new();
    public string TankType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int SensorCount { get; init; }
    public int ActiveSensorCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? LastMaintenanceDate { get; init; }
    public DateTime? NextMaintenanceDate { get; init; }
    public bool IsMaintenanceDue { get; init; }
    public List<SensorDto> Sensors { get; init; } = new();
}

public sealed class LocationDto
{
    public string Building { get; init; } = string.Empty;
    public string Room { get; init; } = string.Empty;
    public string? Zone { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public string FullAddress { get; init; } = string.Empty;
}

public sealed class SensorDto
{
    public Guid Id { get; init; }
    public string SensorType { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string Manufacturer { get; init; } = string.Empty;
    public string SerialNumber { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal? MinValue { get; init; }
    public decimal? MaxValue { get; init; }
    public decimal Accuracy { get; init; }
    public DateTime InstallationDate { get; init; }
    public DateTime? CalibrationDate { get; init; }
    public DateTime? NextCalibrationDate { get; init; }
    public bool IsCalibrationDue { get; init; }
}

