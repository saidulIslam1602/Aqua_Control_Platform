using AquaControl.Domain.Common;
using AquaControl.Domain.Aggregates.TankAggregate;
using AquaControl.Domain.Enums;

namespace AquaControl.Domain.Events;

/// <summary>
/// Raised when a sensor is calibrated.
/// This event captures calibration details including the calibration date, new accuracy value, and optional notes.
/// </summary>
/// <param name="SensorId">The unique identifier of the sensor that was calibrated.</param>
/// <param name="CalibrationDate">The date and time when the calibration was performed.</param>
/// <param name="NewAccuracy">The new accuracy percentage (0-100) after calibration.</param>
/// <param name="Notes">Optional notes about the calibration process or results.</param>
public sealed record SensorCalibratedEvent(
    SensorId SensorId,
    DateTime CalibrationDate,
    decimal NewAccuracy,
    string? Notes
) : DomainEvent
{
    public override string EventType => nameof(SensorCalibratedEvent);
}

/// <summary>
/// Raised when a sensor's measurement range is updated.
/// This event tracks changes to the valid measurement range for the sensor.
/// </summary>
/// <param name="SensorId">The unique identifier of the sensor whose range was updated.</param>
/// <param name="MinValue">The new minimum value in the measurement range.</param>
/// <param name="MaxValue">The new maximum value in the measurement range.</param>
public sealed record SensorRangeUpdatedEvent(
    SensorId SensorId,
    decimal MinValue,
    decimal MaxValue
) : DomainEvent
{
    public override string EventType => nameof(SensorRangeUpdatedEvent);
}

/// <summary>
/// Raised when a sensor is activated and becomes operational.
/// This event indicates that the sensor is now active and ready to take measurements.
/// </summary>
/// <param name="SensorId">The unique identifier of the sensor that was activated.</param>
public sealed record SensorActivatedEvent(SensorId SensorId) : DomainEvent
{
    public override string EventType => nameof(SensorActivatedEvent);
}

/// <summary>
/// Raised when a sensor is deactivated and taken out of service.
/// This event captures the reason for deactivation, which is important for maintenance tracking.
/// </summary>
/// <param name="SensorId">The unique identifier of the sensor that was deactivated.</param>
/// <param name="Reason">The reason why the sensor was deactivated (e.g., "Maintenance", "Malfunction", "Replacement").</param>
public sealed record SensorDeactivatedEvent(
    SensorId SensorId,
    string Reason
) : DomainEvent
{
    public override string EventType => nameof(SensorDeactivatedEvent);
}

/// <summary>
/// Raised when a sensor's operational status changes.
/// This event tracks status transitions such as Online to Offline, Online to Error, etc.
/// </summary>
/// <param name="SensorId">The unique identifier of the sensor whose status changed.</param>
/// <param name="OldStatus">The previous operational status of the sensor.</param>
/// <param name="NewStatus">The new operational status of the sensor.</param>
public sealed record SensorStatusChangedEvent(
    SensorId SensorId,
    SensorStatus OldStatus,
    SensorStatus NewStatus
) : DomainEvent
{
    public override string EventType => nameof(SensorStatusChangedEvent);
}

