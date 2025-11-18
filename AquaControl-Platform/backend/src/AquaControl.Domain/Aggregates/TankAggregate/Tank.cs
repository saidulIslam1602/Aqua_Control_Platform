using AquaControl.Domain.Common;
using AquaControl.Domain.ValueObjects;
using AquaControl.Domain.Events;
using AquaControl.Domain.Enums;

namespace AquaControl.Domain.Aggregates.TankAggregate;

/// <summary>
/// Represents a water tank in the AquaCulture Platform.
/// This is an aggregate root that manages tank properties, sensors, and maintenance operations.
/// </summary>
/// <remarks>
/// <para>
/// The Tank aggregate enforces business rules such as:
/// - A tank must have at least one active sensor to be activated
/// - A tank cannot have more than 10 sensors
/// - The last active sensor cannot be removed from an active tank
/// </para>
/// <para>
/// All state changes are tracked through domain events, enabling event-driven architecture
/// and event sourcing capabilities.
/// </para>
/// </remarks>
public sealed class Tank : AggregateRoot<Guid>
{
    /// <summary>
    /// Internal collection of sensors attached to this tank.
    /// </summary>
    private readonly List<Sensor> _sensors = new();

    /// <summary>
    /// Gets the name of the tank.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the capacity of the tank, including value and unit.
    /// </summary>
    public TankCapacity Capacity { get; private set; } = null!;

    /// <summary>
    /// Gets the physical location of the tank.
    /// </summary>
    public Location Location { get; private set; } = null!;

    /// <summary>
    /// Gets the type of tank (e.g., Freshwater, Saltwater, Breeding).
    /// </summary>
    public TankType TankType { get; private set; }

    /// <summary>
    /// Gets the current operational status of the tank.
    /// </summary>
    public TankStatus Status { get; private set; }

    /// <summary>
    /// Gets the optimal water quality parameters for this tank, if configured.
    /// </summary>
    public WaterQualityParameters? OptimalParameters { get; private set; }

    /// <summary>
    /// Gets the date when maintenance was last completed, if any.
    /// </summary>
    public DateTime? LastMaintenanceDate { get; private set; }

    /// <summary>
    /// Gets the scheduled date for the next maintenance, if any.
    /// </summary>
    public DateTime? NextMaintenanceDate { get; private set; }

    /// <summary>
    /// Gets a read-only collection of all sensors attached to this tank.
    /// </summary>
    /// <value>A read-only list of sensors.</value>
    public IReadOnlyList<Sensor> Sensors => _sensors.AsReadOnly();

    /// <summary>
    /// Initializes a new instance of the <see cref="Tank"/> class.
    /// This parameterless constructor is required by Entity Framework Core for materialization.
    /// </summary>
    private Tank() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tank"/> class with the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier for this tank.</param>
    /// <param name="name">The name of the tank.</param>
    /// <param name="capacity">The capacity of the tank.</param>
    /// <param name="location">The physical location of the tank.</param>
    /// <param name="tankType">The type of tank.</param>
    private Tank(Guid id, string name, TankCapacity capacity, Location location, TankType tankType)
        : base(id)
    {
        Name = name;
        Capacity = capacity;
        Location = location;
        TankType = tankType;
        Status = TankStatus.Inactive;
        
        Apply(new TankCreatedEvent(TankId.Create(Id), name, capacity, location, tankType));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tank"/> class for event replay.
    /// This constructor does not raise domain events and is used exclusively for event sourcing replay.
    /// </summary>
    /// <param name="id">The unique identifier for this tank.</param>
    private Tank(Guid id) : base(id)
    {
        // Initialize with default values that will be overwritten by event replay
        // This constructor does not raise events
    }

    /// <summary>
    /// Creates a new tank instance with the specified properties.
    /// </summary>
    /// <param name="name">The name of the tank. Cannot be null or empty.</param>
    /// <param name="capacity">The capacity of the tank.</param>
    /// <param name="location">The physical location of the tank.</param>
    /// <param name="tankType">The type of tank.</param>
    /// <returns>A new <see cref="Tank"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the tank name is null or empty.</exception>
    /// <remarks>
    /// This factory method validates input and creates a new tank with an automatically generated ID.
    /// The tank is initially created in an Inactive state.
    /// </remarks>
    public static Tank Create(string name, TankCapacity capacity, Location location, TankType tankType)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tank name cannot be empty", nameof(name));

        var tankId = Guid.NewGuid();
        return new Tank(tankId, name, capacity, location, tankType);
    }

    /// <summary>
    /// Creates a tank instance from event replay data.
    /// This method is used when rebuilding aggregate state from events during event sourcing replay.
    /// </summary>
    /// <param name="tankId">The unique identifier for this tank.</param>
    /// <returns>A new <see cref="Tank"/> instance ready for event replay.</returns>
    /// <remarks>
    /// This factory method creates an empty tank instance that can be populated by replaying events.
    /// It does not raise any domain events, as it's used exclusively for event sourcing replay.
    /// The tank is created with minimal default values and will be fully populated by replaying events.
    /// </remarks>
    public static Tank FromEventReplay(Guid tankId)
    {
        // Create tank using event replay constructor (does not raise events)
        // The state will be rebuilt by replaying events via When() methods
        var tank = new Tank(tankId);
        return tank;
    }

    /// <summary>
    /// Updates the name of the tank.
    /// </summary>
    /// <param name="newName">The new name for the tank. Cannot be null or empty.</param>
    /// <exception cref="ArgumentException">Thrown when the new name is null or empty.</exception>
    /// <remarks>
    /// If the new name is the same as the current name, no changes are made and no event is raised.
    /// </remarks>
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Tank name cannot be empty", nameof(newName));

        if (Name == newName) return;

        var oldName = Name;
        Name = newName;
        
        Apply(new TankNameChangedEvent(Id, oldName, newName));
    }

    /// <summary>
    /// Updates the capacity of the tank.
    /// </summary>
    /// <param name="newCapacity">The new capacity for the tank.</param>
    /// <exception cref="ArgumentNullException">Thrown when the new capacity is null.</exception>
    /// <remarks>
    /// If the new capacity is equal to the current capacity, no changes are made and no event is raised.
    /// </remarks>
    public void UpdateCapacity(TankCapacity newCapacity)
    {
        ArgumentNullException.ThrowIfNull(newCapacity);

        if (Capacity.Equals(newCapacity)) return;

        var oldCapacity = Capacity;
        Capacity = newCapacity;
        
        Apply(new TankCapacityChangedEvent(Id, oldCapacity, newCapacity));
    }

    /// <summary>
    /// Relocates the tank to a new physical location.
    /// </summary>
    /// <param name="newLocation">The new location for the tank.</param>
    /// <exception cref="ArgumentNullException">Thrown when the new location is null.</exception>
    /// <remarks>
    /// If the new location is equal to the current location, no changes are made and no event is raised.
    /// </remarks>
    public void Relocate(Location newLocation)
    {
        ArgumentNullException.ThrowIfNull(newLocation);

        if (Location.Equals(newLocation)) return;

        var oldLocation = Location;
        Location = newLocation;
        
        Apply(new TankRelocatedEvent(Id, oldLocation, newLocation));
    }

    /// <summary>
    /// Activates the tank, making it operational.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the tank cannot be activated because it has no active sensors.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Business Rule: A tank must have at least one active sensor before it can be activated.
    /// This ensures that the tank can be monitored once it becomes operational.
    /// </para>
    /// <para>
    /// If the tank is already active, no changes are made and no event is raised.
    /// </para>
    /// </remarks>
    public void Activate()
    {
        if (Status == TankStatus.Active) return;

        // Business rule: Tank must have at least one sensor to be activated
        if (!_sensors.Any(s => s.IsActive))
            throw new InvalidOperationException("Tank must have at least one active sensor to be activated");

        Status = TankStatus.Active;
        Apply(new TankActivatedEvent(Id));
    }

    /// <summary>
    /// Deactivates the tank, making it non-operational.
    /// </summary>
    /// <param name="reason">The reason for deactivation.</param>
    /// <remarks>
    /// If the tank is already inactive, no changes are made and no event is raised.
    /// </remarks>
    public void Deactivate(string reason)
    {
        if (Status == TankStatus.Inactive) return;

        Status = TankStatus.Inactive;
        Apply(new TankDeactivatedEvent(Id, reason));
    }

    /// <summary>
    /// Sets the optimal water quality parameters for this tank.
    /// </summary>
    /// <param name="parameters">The optimal water quality parameters.</param>
    /// <exception cref="ArgumentNullException">Thrown when the parameters are null.</exception>
    public void SetOptimalParameters(WaterQualityParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        OptimalParameters = parameters;
        Apply(new TankOptimalParametersSetEvent(Id, parameters));
    }

    /// <summary>
    /// Schedules maintenance for the tank at the specified date.
    /// </summary>
    /// <param name="maintenanceDate">The date when maintenance should be performed. Must be in the future.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the maintenance date is not in the future.
    /// </exception>
    public void ScheduleMaintenance(DateTime maintenanceDate)
    {
        if (maintenanceDate <= DateTime.UtcNow)
            throw new ArgumentException("Maintenance date must be in the future", nameof(maintenanceDate));

        NextMaintenanceDate = maintenanceDate;
        Apply(new TankMaintenanceScheduledEvent(Id, maintenanceDate));
    }

    /// <summary>
    /// Marks maintenance as completed for the tank.
    /// </summary>
    /// <param name="completionDate">The date when maintenance was completed. Cannot be in the future.</param>
    /// <param name="notes">Optional notes about the maintenance work performed.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the completion date is in the future.
    /// </exception>
    /// <remarks>
    /// This method clears the next maintenance date and records the completion date.
    /// </remarks>
    public void CompleteMaintenance(DateTime completionDate, string notes)
    {
        if (completionDate > DateTime.UtcNow)
            throw new ArgumentException("Completion date cannot be in the future", nameof(completionDate));

        LastMaintenanceDate = completionDate;
        NextMaintenanceDate = null;
        
        Apply(new TankMaintenanceCompletedEvent(Id, completionDate, notes));
    }

    /// <summary>
    /// Adds a sensor to this tank.
    /// </summary>
    /// <param name="sensor">The sensor to add to the tank.</param>
    /// <exception cref="ArgumentNullException">Thrown when the sensor is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the sensor already exists in the tank or when the tank has reached
    /// the maximum number of sensors (10).
    /// </exception>
    /// <remarks>
    /// <para>
    /// Business Rule: A tank cannot have more than 10 sensors. This limit ensures
    /// manageable sensor management and prevents resource overload.
    /// </para>
    /// <para>
    /// Each sensor must have a unique ID within the tank. Attempting to add a sensor
    /// with an ID that already exists will result in an exception.
    /// </para>
    /// </remarks>
    public void AddSensor(Sensor sensor)
    {
        ArgumentNullException.ThrowIfNull(sensor);

        if (_sensors.Any(s => s.Id == sensor.Id))
            throw new InvalidOperationException($"Sensor {sensor.Id} already exists in tank");

        // Business rule: Maximum 10 sensors per tank
        if (_sensors.Count >= 10)
            throw new InvalidOperationException("Tank cannot have more than 10 sensors");

        _sensors.Add(sensor);
        Apply(new SensorAddedToTankEvent(Id, sensor.Id, sensor.SensorType));
    }

    /// <summary>
    /// Removes a sensor from this tank.
    /// </summary>
    /// <param name="sensorId">The unique identifier of the sensor to remove.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the sensor is not found in the tank, or when attempting to remove
    /// the last active sensor from an active tank.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Business Rule: The last active sensor cannot be removed from an active tank.
    /// This ensures that active tanks always have at least one sensor for monitoring.
    /// </para>
    /// <para>
    /// To remove the last active sensor, the tank must first be deactivated.
    /// </para>
    /// </remarks>
    public void RemoveSensor(SensorId sensorId)
    {
        var sensor = _sensors.FirstOrDefault(s => s.Id == sensorId);
        if (sensor == null)
            throw new InvalidOperationException($"Sensor {sensorId} not found in tank");

        // Business rule: Cannot remove last active sensor from active tank
        if (Status == TankStatus.Active && _sensors.Count(s => s.IsActive) == 1 && sensor.IsActive)
            throw new InvalidOperationException("Cannot remove the last active sensor from an active tank");

        _sensors.Remove(sensor);
        Apply(new SensorRemovedFromTankEvent(Id, sensorId));
    }

    /// <summary>
    /// Determines whether maintenance is due for this tank.
    /// </summary>
    /// <returns>
    /// <c>true</c> if maintenance is scheduled and the scheduled date has passed or is today;
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool IsMaintenanceDue()
    {
        return NextMaintenanceDate.HasValue && NextMaintenanceDate.Value <= DateTime.UtcNow;
    }

    /// <summary>
    /// Calculates the time elapsed since the last maintenance was completed.
    /// </summary>
    /// <returns>
    /// A <see cref="TimeSpan"/> representing the time since last maintenance, or <c>null</c>
    /// if maintenance has never been completed.
    /// </returns>
    public TimeSpan? TimeSinceLastMaintenance()
    {
        return LastMaintenanceDate.HasValue ? DateTime.UtcNow - LastMaintenanceDate.Value : null;
    }

    /// <summary>
    /// Determines whether this tank has an active sensor of the specified type.
    /// </summary>
    /// <param name="sensorType">The type of sensor to check for.</param>
    /// <returns>
    /// <c>true</c> if the tank has at least one active sensor of the specified type;
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool HasSensorOfType(SensorType sensorType)
    {
        return _sensors.Any(s => s.SensorType == sensorType && s.IsActive);
    }

    /// <summary>
    /// Gets all active sensors attached to this tank.
    /// </summary>
    /// <returns>An enumerable collection of active sensors.</returns>
    public IEnumerable<Sensor> GetActiveSensors()
    {
        return _sensors.Where(s => s.IsActive);
    }

    #region Event Replay Methods (When Handlers)

    /// <summary>
    /// Applies a domain event to rebuild aggregate state during event replay.
    /// This method is called when loading aggregates from the event store.
    /// </summary>
    /// <param name="domainEvent">The domain event to apply.</param>
    /// <remarks>
    /// These methods rebuild state from events without raising new events or performing business rule validation.
    /// They are used exclusively for event sourcing replay scenarios.
    /// </remarks>
    public void When(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case Events.TankCreatedEvent e:
                When(e);
                break;
            case Events.TankNameChangedEvent e:
                When(e);
                break;
            case Events.TankCapacityChangedEvent e:
                When(e);
                break;
            case Events.TankRelocatedEvent e:
                When(e);
                break;
            case Events.TankActivatedEvent e:
                When(e);
                break;
            case Events.TankDeactivatedEvent e:
                When(e);
                break;
            case Events.TankOptimalParametersSetEvent e:
                When(e);
                break;
            case Events.TankMaintenanceScheduledEvent e:
                When(e);
                break;
            case Events.TankMaintenanceCompletedEvent e:
                When(e);
                break;
            case Events.SensorAddedToTankEvent e:
                When(e);
                break;
            case Events.SensorRemovedFromTankEvent e:
                When(e);
                break;
            default:
                // Unknown event type - log but don't throw to allow forward compatibility
                break;
        }
    }

    /// <summary>
    /// Replays a TankCreatedEvent to rebuild initial tank state.
    /// </summary>
    private void When(Events.TankCreatedEvent e)
    {
        // Rebuild initial state from creation event
        Name = e.Name;
        Capacity = e.Capacity;
        Location = e.Location;
        TankType = e.TankType;
        Status = TankStatus.Inactive;
    }

    /// <summary>
    /// Replays a TankNameChangedEvent to update the tank name.
    /// </summary>
    private void When(Events.TankNameChangedEvent e)
    {
        Name = e.NewName;
    }

    /// <summary>
    /// Replays a TankCapacityChangedEvent to update the tank capacity.
    /// </summary>
    private void When(Events.TankCapacityChangedEvent e)
    {
        Capacity = e.NewCapacity;
    }

    /// <summary>
    /// Replays a TankRelocatedEvent to update the tank location.
    /// </summary>
    private void When(Events.TankRelocatedEvent e)
    {
        Location = e.NewLocation;
    }

    /// <summary>
    /// Replays a TankActivatedEvent to activate the tank.
    /// </summary>
    private void When(Events.TankActivatedEvent e)
    {
        Status = TankStatus.Active;
    }

    /// <summary>
    /// Replays a TankDeactivatedEvent to deactivate the tank.
    /// </summary>
    private void When(Events.TankDeactivatedEvent e)
    {
        Status = TankStatus.Inactive;
    }

    /// <summary>
    /// Replays a TankOptimalParametersSetEvent to set optimal water quality parameters.
    /// </summary>
    private void When(Events.TankOptimalParametersSetEvent e)
    {
        OptimalParameters = e.Parameters;
    }

    /// <summary>
    /// Replays a TankMaintenanceScheduledEvent to schedule maintenance.
    /// </summary>
    private void When(Events.TankMaintenanceScheduledEvent e)
    {
        NextMaintenanceDate = e.ScheduledDate;
    }

    /// <summary>
    /// Replays a TankMaintenanceCompletedEvent to mark maintenance as completed.
    /// </summary>
    private void When(Events.TankMaintenanceCompletedEvent e)
    {
        LastMaintenanceDate = e.CompletionDate;
        NextMaintenanceDate = null;
    }

    /// <summary>
    /// Replays a SensorAddedToTankEvent to add a sensor to the tank.
    /// </summary>
    /// <remarks>
    /// Note: This event only contains SensorId and SensorType. To fully recreate the sensor,
    /// additional information (model, manufacturer, serialNumber, accuracy) would be needed.
    /// The FromEventReplay factory method creates a sensor with default values for missing properties.
    /// Consider enhancing the event to include full sensor details, or load sensor data from read models/snapshots.
    /// </remarks>
    private void When(Events.SensorAddedToTankEvent e)
    {
        // Check if sensor already exists (idempotency)
        if (_sensors.Any(s => s.Id == e.SensorId))
            return;

        // Create sensor from event replay data (with minimal information)
        var sensor = Sensor.FromEventReplay(e.SensorId, e.SensorType);
        _sensors.Add(sensor);
    }

    /// <summary>
    /// Replays a SensorRemovedFromTankEvent to remove a sensor from the tank.
    /// </summary>
    private void When(Events.SensorRemovedFromTankEvent e)
    {
        var sensor = _sensors.FirstOrDefault(s => s.Id == e.SensorId);
        if (sensor != null)
        {
            _sensors.Remove(sensor);
        }
    }

    #endregion
}