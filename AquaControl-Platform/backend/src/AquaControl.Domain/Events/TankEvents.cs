using AquaControl.Domain.Common;
using AquaControl.Domain.Aggregates.TankAggregate;
using AquaControl.Domain.ValueObjects;
using AquaControl.Domain.Enums;

namespace AquaControl.Domain.Events;

/// <summary>
/// Raised when a new tank is created in the system.
/// This event captures the initial state of the tank including its identity, capacity, location, and type.
/// </summary>
/// <param name="TankId">The unique identifier of the newly created tank.</param>
/// <param name="Name">The name assigned to the tank.</param>
/// <param name="Capacity">The capacity of the tank as a value object.</param>
/// <param name="Location">The physical location of the tank as a value object.</param>
/// <param name="TankType">The type classification of the tank (e.g., Freshwater, Saltwater, Breeding).</param>
public sealed record TankCreatedEvent(
    TankId TankId,
    string Name,
    TankCapacity Capacity,
    Location Location,
    TankType TankType
) : DomainEvent
{
    public override string EventType => nameof(TankCreatedEvent);
}

/// <summary>
/// Raised when a tank's name is changed.
/// This event captures both the old and new name values for audit and history tracking purposes.
/// </summary>
/// <param name="TankId">The unique identifier of the tank whose name was changed.</param>
/// <param name="OldName">The previous name of the tank before the change.</param>
/// <param name="NewName">The new name assigned to the tank.</param>
public sealed record TankNameChangedEvent(
    TankId TankId,
    string OldName,
    string NewName
) : DomainEvent
{
    public override string EventType => nameof(TankNameChangedEvent);
}

/// <summary>
/// Raised when a tank's capacity is modified.
/// This event captures both the previous and new capacity values, enabling capacity change tracking and validation.
/// </summary>
/// <param name="TankId">The unique identifier of the tank whose capacity was changed.</param>
/// <param name="OldCapacity">The previous capacity value object before the change.</param>
/// <param name="NewCapacity">The new capacity value object after the change.</param>
public sealed record TankCapacityChangedEvent(
    TankId TankId,
    TankCapacity OldCapacity,
    TankCapacity NewCapacity
) : DomainEvent
{
    public override string EventType => nameof(TankCapacityChangedEvent);
}

/// <summary>
/// Raised when a tank is moved to a different physical location.
/// This event tracks location changes for inventory management and operational purposes.
/// </summary>
/// <param name="TankId">The unique identifier of the tank that was relocated.</param>
/// <param name="OldLocation">The previous location value object before the move.</param>
/// <param name="NewLocation">The new location value object after the move.</param>
public sealed record TankRelocatedEvent(
    TankId TankId,
    Location OldLocation,
    Location NewLocation
) : DomainEvent
{
    public override string EventType => nameof(TankRelocatedEvent);
}

/// <summary>
/// Raised when a tank is activated and becomes operational.
/// This event indicates that the tank is now active and ready for use in the aquaculture system.
/// </summary>
/// <param name="TankId">The unique identifier of the tank that was activated.</param>
public sealed record TankActivatedEvent(TankId TankId) : DomainEvent
{
    public override string EventType => nameof(TankActivatedEvent);
}

/// <summary>
/// Raised when a tank is deactivated and taken out of service.
/// This event captures the reason for deactivation, which is important for maintenance tracking and operational history.
/// </summary>
/// <param name="TankId">The unique identifier of the tank that was deactivated.</param>
/// <param name="Reason">The reason why the tank was deactivated (e.g., "Maintenance", "Repair", "Seasonal shutdown").</param>
public sealed record TankDeactivatedEvent(
    TankId TankId,
    string Reason
) : DomainEvent
{
    public override string EventType => nameof(TankDeactivatedEvent);
}

/// <summary>
/// Raised when optimal water quality parameters are set for a tank.
/// This event captures the target parameters that should be maintained for the tank's aquatic environment.
/// </summary>
/// <param name="TankId">The unique identifier of the tank for which parameters were set.</param>
/// <param name="Parameters">The water quality parameters value object containing optimal values (pH, temperature, dissolved oxygen, etc.).</param>
public sealed record TankOptimalParametersSetEvent(
    TankId TankId,
    WaterQualityParameters Parameters
) : DomainEvent
{
    public override string EventType => nameof(TankOptimalParametersSetEvent);
}

/// <summary>
/// Raised when maintenance is scheduled for a tank.
/// This event enables maintenance tracking, scheduling systems, and reminder notifications.
/// </summary>
/// <param name="TankId">The unique identifier of the tank for which maintenance was scheduled.</param>
/// <param name="ScheduledDate">The date and time when the maintenance is scheduled to occur.</param>
public sealed record TankMaintenanceScheduledEvent(
    TankId TankId,
    DateTime ScheduledDate
) : DomainEvent
{
    public override string EventType => nameof(TankMaintenanceScheduledEvent);
}

/// <summary>
/// Raised when scheduled maintenance for a tank is completed.
/// This event captures completion details including the completion date and any notes from the maintenance work.
/// </summary>
/// <param name="TankId">The unique identifier of the tank for which maintenance was completed.</param>
/// <param name="CompletionDate">The date and time when the maintenance was actually completed.</param>
/// <param name="Notes">Optional notes or observations recorded during the maintenance work.</param>
public sealed record TankMaintenanceCompletedEvent(
    TankId TankId,
    DateTime CompletionDate,
    string Notes
) : DomainEvent
{
    public override string EventType => nameof(TankMaintenanceCompletedEvent);
}

/// <summary>
/// Raised when a sensor is added to a tank.
/// This event tracks sensor installation and enables sensor inventory management and monitoring setup.
/// </summary>
/// <param name="TankId">The unique identifier of the tank to which the sensor was added.</param>
/// <param name="SensorId">The unique identifier of the sensor that was added to the tank.</param>
/// <param name="SensorType">The type of sensor that was added (e.g., Temperature, pH, DissolvedOxygen).</param>
public sealed record SensorAddedToTankEvent(
    TankId TankId,
    SensorId SensorId,
    SensorType SensorType
) : DomainEvent
{
    public override string EventType => nameof(SensorAddedToTankEvent);
}

/// <summary>
/// Raised when a sensor is removed from a tank.
/// This event tracks sensor removal for inventory management and monitoring system updates.
/// </summary>
/// <param name="TankId">The unique identifier of the tank from which the sensor was removed.</param>
/// <param name="SensorId">The unique identifier of the sensor that was removed from the tank.</param>
public sealed record SensorRemovedFromTankEvent(
    TankId TankId,
    SensorId SensorId
) : DomainEvent
{
    public override string EventType => nameof(SensorRemovedFromTankEvent);
}