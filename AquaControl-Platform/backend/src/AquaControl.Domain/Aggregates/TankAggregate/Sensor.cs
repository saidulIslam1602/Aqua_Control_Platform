using AquaControl.Domain.Common;
using AquaControl.Domain.Enums;
using AquaControl.Domain.Events;

namespace AquaControl.Domain.Aggregates.TankAggregate;

/// <summary>
/// Represents a sensor entity within the Tank aggregate.
/// Sensors are child entities that belong to a Tank aggregate root and monitor various water quality parameters.
/// </summary>
/// <remarks>
/// <para>
/// This is a child entity within the Tank aggregate. Sensors are managed through the Tank aggregate root
/// and cannot be directly modified from outside the aggregate. All sensor operations should go through
/// the Tank aggregate root methods (e.g., <c>Tank.AddSensor()</c>, <c>Tank.RemoveSensor()</c>).
/// </para>
/// <para>
/// Sensors track calibration schedules, operational status, and measurement ranges. Each sensor type
/// has default ranges and calibration intervals that are automatically set during creation.
/// </para>
/// </remarks>
public sealed class Sensor : Entity<SensorId>
{
    /// <summary>
    /// Gets the type of sensor (e.g., Temperature, pH, DissolvedOxygen).
    /// </summary>
    public SensorType SensorType { get; private set; }

    /// <summary>
    /// Gets the model name of the sensor.
    /// </summary>
    public string Model { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the manufacturer of the sensor.
    /// </summary>
    public string Manufacturer { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the unique serial number of the sensor.
    /// </summary>
    public string SerialNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the date when the sensor was installed.
    /// </summary>
    public DateTime InstallationDate { get; private set; }

    /// <summary>
    /// Gets the date when the sensor was last calibrated, if any.
    /// </summary>
    public DateTime? CalibrationDate { get; private set; }

    /// <summary>
    /// Gets the scheduled date for the next calibration, if any.
    /// </summary>
    public DateTime? NextCalibrationDate { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the sensor is currently active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets the current operational status of the sensor.
    /// </summary>
    public SensorStatus Status { get; private set; }

    /// <summary>
    /// Gets the minimum value in the sensor's measurement range.
    /// </summary>
    public decimal? MinValue { get; private set; }

    /// <summary>
    /// Gets the maximum value in the sensor's measurement range.
    /// </summary>
    public decimal? MaxValue { get; private set; }

    /// <summary>
    /// Gets the accuracy percentage of the sensor (0-100).
    /// </summary>
    public decimal Accuracy { get; private set; }

    /// <summary>
    /// Gets optional notes about the sensor.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Sensor"/> class.
    /// This parameterless constructor is required by Entity Framework Core for materialization.
    /// </summary>
    private Sensor() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Sensor"/> class with the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier for this sensor.</param>
    /// <param name="sensorType">The type of sensor.</param>
    /// <param name="model">The model name of the sensor.</param>
    /// <param name="manufacturer">The manufacturer of the sensor.</param>
    /// <param name="serialNumber">The unique serial number of the sensor.</param>
    /// <param name="accuracy">The accuracy percentage of the sensor (0-100).</param>
    /// <remarks>
    /// The sensor is initialized as active with Online status, and default measurement ranges
    /// are set based on the sensor type.
    /// </remarks>
    private Sensor(SensorId id, SensorType sensorType, string model, string manufacturer, 
        string serialNumber, decimal accuracy) : base(id)
    {
        SensorType = sensorType;
        Model = model;
        Manufacturer = manufacturer;
        SerialNumber = serialNumber;
        Accuracy = accuracy;
        InstallationDate = DateTime.UtcNow;
        IsActive = true;
        Status = SensorStatus.Online;
        
        SetDefaultRanges();
    }

    /// <summary>
    /// Creates a new sensor instance with the specified properties.
    /// </summary>
    /// <param name="sensorType">The type of sensor.</param>
    /// <param name="model">The model name of the sensor. Cannot be null or empty.</param>
    /// <param name="manufacturer">The manufacturer of the sensor. Cannot be null or empty.</param>
    /// <param name="serialNumber">The unique serial number of the sensor. Cannot be null or empty.</param>
    /// <param name="accuracy">The accuracy percentage of the sensor (0-100).</param>
    /// <returns>A new <see cref="Sensor"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when model, manufacturer, or serialNumber is null or empty, or when accuracy is not between 0 and 100.
    /// </exception>
    /// <remarks>
    /// This factory method validates input and creates a new sensor with an automatically generated ID.
    /// The sensor is initially created as active with Online status, and default measurement ranges
    /// are automatically set based on the sensor type.
    /// </remarks>
    public static Sensor Create(SensorType sensorType, string model, string manufacturer, 
        string serialNumber, decimal accuracy)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Model cannot be empty", nameof(model));

        if (string.IsNullOrWhiteSpace(manufacturer))
            throw new ArgumentException("Manufacturer cannot be empty", nameof(manufacturer));

        if (string.IsNullOrWhiteSpace(serialNumber))
            throw new ArgumentException("Serial number cannot be empty", nameof(serialNumber));

        if (accuracy <= 0 || accuracy > 100)
            throw new ArgumentException("Accuracy must be between 0 and 100", nameof(accuracy));

        var sensorId = SensorId.Create();
        return new Sensor(sensorId, sensorType, model, manufacturer, serialNumber, accuracy);
    }

    /// <summary>
    /// Calibrates the sensor with a new accuracy value and sets the next calibration date.
    /// </summary>
    /// <param name="calibrationDate">The date when calibration was performed. Cannot be in the future.</param>
    /// <param name="newAccuracy">The new accuracy percentage (0-100).</param>
    /// <param name="notes">Optional notes about the calibration.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the calibration date is in the future or when accuracy is not between 0 and 100.
    /// </exception>
    /// <remarks>
    /// The next calibration date is automatically calculated based on the sensor type's calibration interval.
    /// Different sensor types have different calibration intervals (e.g., pH sensors require calibration every 3 months).
    /// </remarks>
    public void Calibrate(DateTime calibrationDate, decimal newAccuracy, string? notes = null)
    {
        if (calibrationDate > DateTime.UtcNow)
            throw new ArgumentException("Calibration date cannot be in the future", nameof(calibrationDate));

        if (newAccuracy <= 0 || newAccuracy > 100)
            throw new ArgumentException("Accuracy must be between 0 and 100", nameof(newAccuracy));

        CalibrationDate = calibrationDate;
        Accuracy = newAccuracy;
        Notes = notes;
        
        // Set next calibration date based on sensor type
        NextCalibrationDate = calibrationDate.AddMonths(GetCalibrationIntervalMonths());
        
        AddDomainEvent(new SensorCalibratedEvent(Id, calibrationDate, newAccuracy, notes));
    }

    /// <summary>
    /// Sets the measurement range for the sensor.
    /// </summary>
    /// <param name="minValue">The minimum value in the measurement range.</param>
    /// <param name="maxValue">The maximum value in the measurement range.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when minValue is greater than or equal to maxValue.
    /// </exception>
    /// <remarks>
    /// The measurement range defines the valid values that the sensor can measure.
    /// Values outside this range may indicate sensor malfunction or invalid readings.
    /// </remarks>
    public void SetRange(decimal minValue, decimal maxValue)
    {
        if (minValue >= maxValue)
            throw new ArgumentException("Min value must be less than max value");

        MinValue = minValue;
        MaxValue = maxValue;
        
        AddDomainEvent(new SensorRangeUpdatedEvent(Id, minValue, maxValue));
    }

    /// <summary>
    /// Activates the sensor, making it operational.
    /// </summary>
    /// <remarks>
    /// If the sensor is already active, no changes are made and no event is raised.
    /// When activated, the sensor status is set to Online.
    /// </remarks>
    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        Status = SensorStatus.Online;
        
        AddDomainEvent(new SensorActivatedEvent(Id));
    }

    /// <summary>
    /// Deactivates the sensor, making it non-operational.
    /// </summary>
    /// <param name="reason">The reason for deactivation, which is stored in the Notes property.</param>
    /// <remarks>
    /// If the sensor is already inactive, no changes are made and no event is raised.
    /// When deactivated, the sensor status is set to Offline and the reason is recorded.
    /// </remarks>
    public void Deactivate(string reason)
    {
        if (!IsActive) return;

        IsActive = false;
        Status = SensorStatus.Offline;
        Notes = reason;
        
        AddDomainEvent(new SensorDeactivatedEvent(Id, reason));
    }

    /// <summary>
    /// Updates the operational status of the sensor.
    /// </summary>
    /// <param name="newStatus">The new status for the sensor.</param>
    /// <remarks>
    /// If the new status is the same as the current status, no changes are made and no event is raised.
    /// This method is used to update status based on sensor readings or system events.
    /// </remarks>
    public void UpdateStatus(SensorStatus newStatus)
    {
        if (Status == newStatus) return;

        var oldStatus = Status;
        Status = newStatus;
        
        AddDomainEvent(new SensorStatusChangedEvent(Id, oldStatus, newStatus));
    }

    /// <summary>
    /// Determines whether calibration is due for this sensor.
    /// </summary>
    /// <returns>
    /// <c>true</c> if calibration is scheduled and the scheduled date has passed or is today;
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool IsCalibrationDue()
    {
        return NextCalibrationDate.HasValue && NextCalibrationDate.Value <= DateTime.UtcNow;
    }

    /// <summary>
    /// Determines whether a measurement value is within the sensor's valid range.
    /// </summary>
    /// <param name="value">The measurement value to check.</param>
    /// <returns>
    /// <c>true</c> if the value is within range, or if no range is defined;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// If no range is defined (MinValue or MaxValue is null), this method returns <c>true</c>
    /// to allow all values. This is useful for sensors that don't have predefined ranges.
    /// </remarks>
    public bool IsValueInRange(decimal value)
    {
        if (!MinValue.HasValue || !MaxValue.HasValue) return true;
        return value >= MinValue.Value && value <= MaxValue.Value;
    }

    /// <summary>
    /// Sets default measurement ranges based on the sensor type.
    /// </summary>
    /// <remarks>
    /// Each sensor type has appropriate default ranges for its measurement domain:
    /// - Temperature: -10°C to 50°C
    /// - pH: 0 to 14
    /// - DissolvedOxygen: 0 to 20 mg/L
    /// - Salinity: 0 to 50 ppt
    /// - Turbidity: 0 to 1000 NTU
    /// - Ammonia: 0 to 10 mg/L
    /// - Nitrite: 0 to 5 mg/L
    /// - Nitrate: 0 to 100 mg/L
    /// </remarks>
    private void SetDefaultRanges()
    {
        var ranges = SensorType switch
        {
            SensorType.Temperature => (-10m, 50m), // Celsius
            SensorType.pH => (0m, 14m),
            SensorType.DissolvedOxygen => (0m, 20m), // mg/L
            SensorType.Salinity => (0m, 50m), // ppt
            SensorType.Turbidity => (0m, 1000m), // NTU
            SensorType.Ammonia => (0m, 10m), // mg/L
            SensorType.Nitrite => (0m, 5m), // mg/L
            SensorType.Nitrate => (0m, 100m), // mg/L
            SensorType.Phosphate => (0m, 10m), // mg/L
            SensorType.Alkalinity => (0m, 300m), // mg/L CaCO3
            _ => (0m, 100m)
        };
        
        MinValue = ranges.Item1;
        MaxValue = ranges.Item2;
    }

    /// <summary>
    /// Gets the calibration interval in months for the sensor type.
    /// </summary>
    /// <returns>The number of months between calibrations for this sensor type.</returns>
    /// <remarks>
    /// Different sensor types require different calibration frequencies:
    /// - pH, Turbidity, Ammonia, Nitrite: 3 months
    /// - DissolvedOxygen, Salinity, Nitrate: 6 months
    /// - Temperature: 12 months
    /// - Default: 6 months
    /// </remarks>
    private int GetCalibrationIntervalMonths()
    {
        return SensorType switch
        {
            SensorType.pH => 3,
            SensorType.DissolvedOxygen => 6,
            SensorType.Temperature => 12,
            SensorType.Salinity => 6,
            SensorType.Turbidity => 3,
            SensorType.Ammonia => 3,
            SensorType.Nitrite => 3,
            SensorType.Nitrate => 6,
            _ => 6
        };
    }
}