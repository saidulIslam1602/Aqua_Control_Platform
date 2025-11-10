# Phase 1 - Step 1: Domain Layer (DDD Core) - Foundation Layer

## 🏗️ Architecture Overview

This implementation follows **Domain-Driven Design (DDD)**, **Clean Architecture**, **CQRS**, and **Event Sourcing** patterns used by enterprises like Netflix, Uber, and Microsoft.

### Architecture Layers
```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   REST API      │  │    GraphQL      │  │   SignalR    │ │
│  │   Controllers   │  │    Resolvers    │  │     Hubs     │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                          │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │    Commands     │  │     Queries     │  │   Handlers   │ │
│  │   (CQRS Write)  │  │   (CQRS Read)   │  │  (MediatR)   │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                    Domain Layer                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Aggregates    │  │  Domain Events  │  │  Value Objs  │ │
│  │   (Entities)    │  │   (Events)      │  │  (Immutable) │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                 Infrastructure Layer                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │  Event Store    │  │   Read Models   │  │  External    │ │
│  │  (Write Side)   │  │   (Read Side)   │  │  Services    │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

---

## 📁 Project Structure

```bash
mkdir -p AquaControl-Platform/backend/{src,tests,docs,scripts}
cd AquaControl-Platform/backend

# Create sophisticated project structure
mkdir -p src/{
  AquaControl.Domain/{Aggregates,Events,ValueObjects,Specifications,Services},
  AquaControl.Application/{Commands,Queries,Handlers,Behaviors,Services,DTOs},
  AquaControl.Infrastructure/{Persistence,EventStore,ReadModels,ExternalServices,Messaging},
  AquaControl.Presentation/{API,GraphQL,SignalR,Middleware},
  AquaControl.Shared/{Common,Extensions,Utilities}
}

mkdir -p tests/{
  AquaControl.Domain.Tests,
  AquaControl.Application.Tests,
  AquaControl.Infrastructure.Tests,
  AquaControl.API.Tests/{Unit,Integration,Performance}
}
```

---

## 🎯 Step 1: Domain Layer (DDD Core)

### File 1: Base Domain Entity
**File:** `src/AquaControl.Domain/Common/Entity.cs`
```csharp
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaControl.Domain.Common;

public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected Entity(TId id)
    {
        Id = id;
    }

    protected Entity() { } // For EF Core

    public TId Id { get; protected set; } = default!;

    [NotMapped]
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public bool Equals(Entity<TId>? other)
    {
        return other is not null && Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> entity && Equals(entity);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !Equals(left, right);
    }
}
```

### File 2: Aggregate Root Base
**File:** `src/AquaControl.Domain/Common/AggregateRoot.cs`
```csharp
namespace AquaControl.Domain.Common;

public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    protected AggregateRoot(TId id) : base(id) { }
    protected AggregateRoot() { } // For EF Core

    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public long Version { get; protected set; } = 0;

    protected void IncrementVersion()
    {
        Version++;
        UpdatedAt = DateTime.UtcNow;
    }

    protected void Apply(IDomainEvent domainEvent)
    {
        AddDomainEvent(domainEvent);
        IncrementVersion();
    }
}
```

### File 3: Domain Event Interface
**File:** `src/AquaControl.Domain/Common/IDomainEvent.cs`
```csharp
using MediatR;

namespace AquaControl.Domain.Common;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}
```

### File 4: Value Objects Base
**File:** `src/AquaControl.Domain/Common/ValueObject.cs`
```csharp
namespace AquaControl.Domain.Common;

public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public bool Equals(ValueObject? other)
    {
        return Equals((object?)other);
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !Equals(left, right);
    }
}
```

### File 5: Tank Value Objects
**File:** `src/AquaControl.Domain/ValueObjects/TankCapacity.cs`
```csharp
using AquaControl.Domain.Common;

namespace AquaControl.Domain.ValueObjects;

public sealed class TankCapacity : ValueObject
{
    public decimal Value { get; }
    public string Unit { get; }

    private TankCapacity(decimal value, string unit)
    {
        Value = value;
        Unit = unit;
    }

    public static TankCapacity Create(decimal value, string unit = "L")
    {
        if (value <= 0)
            throw new ArgumentException("Tank capacity must be positive", nameof(value));

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Unit cannot be empty", nameof(unit));

        return new TankCapacity(value, unit);
    }

    public TankCapacity ConvertTo(string targetUnit)
    {
        var convertedValue = targetUnit.ToUpperInvariant() switch
        {
            "L" when Unit.ToUpperInvariant() == "ML" => Value * 1000,
            "ML" when Unit.ToUpperInvariant() == "L" => Value / 1000,
            "GAL" when Unit.ToUpperInvariant() == "L" => Value * 0.264172m,
            "L" when Unit.ToUpperInvariant() == "GAL" => Value / 0.264172m,
            _ when Unit.ToUpperInvariant() == targetUnit.ToUpperInvariant() => Value,
            _ => throw new ArgumentException($"Cannot convert from {Unit} to {targetUnit}")
        };

        return new TankCapacity(convertedValue, targetUnit);
    }

    public override string ToString() => $"{Value:F2} {Unit}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Unit.ToUpperInvariant();
    }
}
```

**File:** `src/AquaControl.Domain/ValueObjects/Location.cs`
```csharp
using AquaControl.Domain.Common;

namespace AquaControl.Domain.ValueObjects;

public sealed class Location : ValueObject
{
    public string Building { get; }
    public string Room { get; }
    public string? Zone { get; }
    public decimal? Latitude { get; }
    public decimal? Longitude { get; }

    private Location(string building, string room, string? zone, decimal? latitude, decimal? longitude)
    {
        Building = building;
        Room = room;
        Zone = zone;
        Latitude = latitude;
        Longitude = longitude;
    }

    public static Location Create(string building, string room, string? zone = null, 
        decimal? latitude = null, decimal? longitude = null)
    {
        if (string.IsNullOrWhiteSpace(building))
            throw new ArgumentException("Building cannot be empty", nameof(building));

        if (string.IsNullOrWhiteSpace(room))
            throw new ArgumentException("Room cannot be empty", nameof(room));

        if (latitude.HasValue && (latitude < -90 || latitude > 90))
            throw new ArgumentException("Latitude must be between -90 and 90", nameof(latitude));

        if (longitude.HasValue && (longitude < -180 || longitude > 180))
            throw new ArgumentException("Longitude must be between -180 and 180", nameof(longitude));

        return new Location(building, room, zone, latitude, longitude);
    }

    public string GetFullAddress()
    {
        var parts = new List<string> { Building, Room };
        if (!string.IsNullOrWhiteSpace(Zone))
            parts.Add(Zone);
        return string.Join(", ", parts);
    }

    public double? DistanceTo(Location other)
    {
        if (!Latitude.HasValue || !Longitude.HasValue || 
            !other.Latitude.HasValue || !other.Longitude.HasValue)
            return null;

        // Haversine formula for distance calculation
        const double R = 6371; // Earth's radius in kilometers
        
        var lat1Rad = (double)Latitude.Value * Math.PI / 180;
        var lat2Rad = (double)other.Latitude.Value * Math.PI / 180;
        var deltaLatRad = ((double)other.Latitude.Value - (double)Latitude.Value) * Math.PI / 180;
        var deltaLonRad = ((double)other.Longitude.Value - (double)Longitude.Value) * Math.PI / 180;

        var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return R * c * 1000; // Return distance in meters
    }

    public override string ToString() => GetFullAddress();

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Building.ToUpperInvariant();
        yield return Room.ToUpperInvariant();
        yield return Zone?.ToUpperInvariant() ?? string.Empty;
        yield return Latitude ?? 0;
        yield return Longitude ?? 0;
    }
}
```

### File 6: Tank Aggregate Root
**File:** `src/AquaControl.Domain/Aggregates/TankAggregate/Tank.cs`
```csharp
using AquaControl.Domain.Common;
using AquaControl.Domain.ValueObjects;
using AquaControl.Domain.Events;
using AquaControl.Domain.Enums;

namespace AquaControl.Domain.Aggregates.TankAggregate;

public sealed class Tank : AggregateRoot<TankId>
{
    private readonly List<Sensor> _sensors = new();

    public string Name { get; private set; } = string.Empty;
    public TankCapacity Capacity { get; private set; } = null!;
    public Location Location { get; private set; } = null!;
    public TankType TankType { get; private set; }
    public TankStatus Status { get; private set; }
    public WaterQualityParameters? OptimalParameters { get; private set; }
    public DateTime? LastMaintenanceDate { get; private set; }
    public DateTime? NextMaintenanceDate { get; private set; }

    public IReadOnlyList<Sensor> Sensors => _sensors.AsReadOnly();

    private Tank() { } // For EF Core

    private Tank(TankId id, string name, TankCapacity capacity, Location location, TankType tankType)
        : base(id)
    {
        Name = name;
        Capacity = capacity;
        Location = location;
        TankType = tankType;
        Status = TankStatus.Inactive;
        
        Apply(new TankCreatedEvent(Id, name, capacity, location, tankType));
    }

    public static Tank Create(string name, TankCapacity capacity, Location location, TankType tankType)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tank name cannot be empty", nameof(name));

        var tankId = TankId.Create();
        return new Tank(tankId, name, capacity, location, tankType);
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Tank name cannot be empty", nameof(newName));

        if (Name == newName) return;

        var oldName = Name;
        Name = newName;
        
        Apply(new TankNameChangedEvent(Id, oldName, newName));
    }

    public void UpdateCapacity(TankCapacity newCapacity)
    {
        ArgumentNullException.ThrowIfNull(newCapacity);

        if (Capacity.Equals(newCapacity)) return;

        var oldCapacity = Capacity;
        Capacity = newCapacity;
        
        Apply(new TankCapacityChangedEvent(Id, oldCapacity, newCapacity));
    }

    public void Relocate(Location newLocation)
    {
        ArgumentNullException.ThrowIfNull(newLocation);

        if (Location.Equals(newLocation)) return;

        var oldLocation = Location;
        Location = newLocation;
        
        Apply(new TankRelocatedEvent(Id, oldLocation, newLocation));
    }

    public void Activate()
    {
        if (Status == TankStatus.Active) return;

        // Business rule: Tank must have at least one sensor to be activated
        if (!_sensors.Any(s => s.IsActive))
            throw new InvalidOperationException("Tank must have at least one active sensor to be activated");

        Status = TankStatus.Active;
        Apply(new TankActivatedEvent(Id));
    }

    public void Deactivate(string reason)
    {
        if (Status == TankStatus.Inactive) return;

        Status = TankStatus.Inactive;
        Apply(new TankDeactivatedEvent(Id, reason));
    }

    public void SetOptimalParameters(WaterQualityParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        OptimalParameters = parameters;
        Apply(new TankOptimalParametersSetEvent(Id, parameters));
    }

    public void ScheduleMaintenance(DateTime maintenanceDate)
    {
        if (maintenanceDate <= DateTime.UtcNow)
            throw new ArgumentException("Maintenance date must be in the future", nameof(maintenanceDate));

        NextMaintenanceDate = maintenanceDate;
        Apply(new TankMaintenanceScheduledEvent(Id, maintenanceDate));
    }

    public void CompleteMaintenance(DateTime completionDate, string notes)
    {
        if (completionDate > DateTime.UtcNow)
            throw new ArgumentException("Completion date cannot be in the future", nameof(completionDate));

        LastMaintenanceDate = completionDate;
        NextMaintenanceDate = null;
        
        Apply(new TankMaintenanceCompletedEvent(Id, completionDate, notes));
    }

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

    public bool IsMaintenanceDue()
    {
        return NextMaintenanceDate.HasValue && NextMaintenanceDate.Value <= DateTime.UtcNow;
    }

    public TimeSpan? TimeSinceLastMaintenance()
    {
        return LastMaintenanceDate.HasValue ? DateTime.UtcNow - LastMaintenanceDate.Value : null;
    }

    public bool HasSensorOfType(SensorType sensorType)
    {
        return _sensors.Any(s => s.SensorType == sensorType && s.IsActive);
    }

    public IEnumerable<Sensor> GetActiveSensors()
    {
        return _sensors.Where(s => s.IsActive);
    }
}
```

### File 7: Tank ID Value Object
**File:** `src/AquaControl.Domain/Aggregates/TankAggregate/TankId.cs`
```csharp
using AquaControl.Domain.Common;

namespace AquaControl.Domain.Aggregates.TankAggregate;

public sealed class TankId : ValueObject
{
    public Guid Value { get; }

    private TankId(Guid value)
    {
        Value = value;
    }

    public static TankId Create() => new(Guid.NewGuid());
    public static TankId Create(Guid value) => new(value);

    public static implicit operator Guid(TankId tankId) => tankId.Value;
    public static implicit operator TankId(Guid value) => new(value);

    public override string ToString() => Value.ToString();

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

### File 8: Sensor Entity
**File:** `src/AquaControl.Domain/Aggregates/TankAggregate/Sensor.cs`
```csharp
using AquaControl.Domain.Common;
using AquaControl.Domain.Enums;
using AquaControl.Domain.Events;

namespace AquaControl.Domain.Aggregates.TankAggregate;

public sealed class Sensor : Entity<SensorId>
{
    public SensorType SensorType { get; private set; }
    public string Model { get; private set; } = string.Empty;
    public string Manufacturer { get; private set; } = string.Empty;
    public string SerialNumber { get; private set; } = string.Empty;
    public DateTime InstallationDate { get; private set; }
    public DateTime? CalibrationDate { get; private set; }
    public DateTime? NextCalibrationDate { get; private set; }
    public bool IsActive { get; private set; }
    public SensorStatus Status { get; private set; }
    public decimal? MinValue { get; private set; }
    public decimal? MaxValue { get; private set; }
    public decimal Accuracy { get; private set; }
    public string? Notes { get; private set; }

    private Sensor() { } // For EF Core

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

    public void SetRange(decimal minValue, decimal maxValue)
    {
        if (minValue >= maxValue)
            throw new ArgumentException("Min value must be less than max value");

        MinValue = minValue;
        MaxValue = maxValue;
        
        AddDomainEvent(new SensorRangeUpdatedEvent(Id, minValue, maxValue));
    }

    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        Status = SensorStatus.Online;
        
        AddDomainEvent(new SensorActivatedEvent(Id));
    }

    public void Deactivate(string reason)
    {
        if (!IsActive) return;

        IsActive = false;
        Status = SensorStatus.Offline;
        Notes = reason;
        
        AddDomainEvent(new SensorDeactivatedEvent(Id, reason));
    }

    public void UpdateStatus(SensorStatus newStatus)
    {
        if (Status == newStatus) return;

        var oldStatus = Status;
        Status = newStatus;
        
        AddDomainEvent(new SensorStatusChangedEvent(Id, oldStatus, newStatus));
    }

    public bool IsCalibrationDue()
    {
        return NextCalibrationDate.HasValue && NextCalibrationDate.Value <= DateTime.UtcNow;
    }

    public bool IsValueInRange(decimal value)
    {
        if (!MinValue.HasValue || !MaxValue.HasValue) return true;
        return value >= MinValue.Value && value <= MaxValue.Value;
    }

    private void SetDefaultRanges()
    {
        (MinValue, MaxValue) = SensorType switch
        {
            SensorType.Temperature => (-10m, 50m), // Celsius
            SensorType.pH => (0m, 14m),
            SensorType.DissolvedOxygen => (0m, 20m), // mg/L
            SensorType.Salinity => (0m, 50m), // ppt
            SensorType.Turbidity => (0m, 1000m), // NTU
            SensorType.Ammonia => (0m, 10m), // mg/L
            SensorType.Nitrite => (0m, 5m), // mg/L
            SensorType.Nitrate => (0m, 100m), // mg/L
            _ => (null, null)
        };
    }

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
```

### File 9: Enums
**File:** `src/AquaControl.Domain/Enums/TankType.cs`
```csharp
namespace AquaControl.Domain.Enums;

public enum TankType
{
    Freshwater = 1,
    Saltwater = 2,
    Breeding = 3,
    Quarantine = 4,
    Nursery = 5,
    Grow_out = 6,
    Broodstock = 7
}
```

**File:** `src/AquaControl.Domain/Enums/TankStatus.cs`
```csharp
namespace AquaControl.Domain.Enums;

public enum TankStatus
{
    Inactive = 0,
    Active = 1,
    Maintenance = 2,
    Emergency = 3,
    Cleaning = 4
}
```

**File:** `src/AquaControl.Domain/Enums/SensorType.cs`
```csharp
namespace AquaControl.Domain.Enums;

public enum SensorType
{
    Temperature = 1,
    pH = 2,
    DissolvedOxygen = 3,
    Salinity = 4,
    Turbidity = 5,
    Ammonia = 6,
    Nitrite = 7,
    Nitrate = 8,
    Phosphate = 9,
    Alkalinity = 10
}
```

**File:** `src/AquaControl.Domain/Enums/SensorStatus.cs`
```csharp
namespace AquaControl.Domain.Enums;

public enum SensorStatus
{
    Offline = 0,
    Online = 1,
    Calibrating = 2,
    Error = 3,
    Maintenance = 4
}
```

### File 10: Domain Events
**File:** `src/AquaControl.Domain/Events/TankEvents.cs`
```csharp
using AquaControl.Domain.Common;
using AquaControl.Domain.Aggregates.TankAggregate;
using AquaControl.Domain.ValueObjects;
using AquaControl.Domain.Enums;

namespace AquaControl.Domain.Events;

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

public sealed record TankNameChangedEvent(
    TankId TankId,
    string OldName,
    string NewName
) : DomainEvent
{
    public override string EventType => nameof(TankNameChangedEvent);
}

public sealed record TankCapacityChangedEvent(
    TankId TankId,
    TankCapacity OldCapacity,
    TankCapacity NewCapacity
) : DomainEvent
{
    public override string EventType => nameof(TankCapacityChangedEvent);
}

public sealed record TankRelocatedEvent(
    TankId TankId,
    Location OldLocation,
    Location NewLocation
) : DomainEvent
{
    public override string EventType => nameof(TankRelocatedEvent);
}

public sealed record TankActivatedEvent(TankId TankId) : DomainEvent
{
    public override string EventType => nameof(TankActivatedEvent);
}

public sealed record TankDeactivatedEvent(
    TankId TankId,
    string Reason
) : DomainEvent
{
    public override string EventType => nameof(TankDeactivatedEvent);
}

public sealed record TankOptimalParametersSetEvent(
    TankId TankId,
    WaterQualityParameters Parameters
) : DomainEvent
{
    public override string EventType => nameof(TankOptimalParametersSetEvent);
}

public sealed record TankMaintenanceScheduledEvent(
    TankId TankId,
    DateTime ScheduledDate
) : DomainEvent
{
    public override string EventType => nameof(TankMaintenanceScheduledEvent);
}

public sealed record TankMaintenanceCompletedEvent(
    TankId TankId,
    DateTime CompletionDate,
    string Notes
) : DomainEvent
{
    public override string EventType => nameof(TankMaintenanceCompletedEvent);
}

public sealed record SensorAddedToTankEvent(
    TankId TankId,
    SensorId SensorId,
    SensorType SensorType
) : DomainEvent
{
    public override string EventType => nameof(SensorAddedToTankEvent);
}

public sealed record SensorRemovedFromTankEvent(
    TankId TankId,
    SensorId SensorId
) : DomainEvent
{
    public override string EventType => nameof(SensorRemovedFromTankEvent);
}
```

### File 11: Water Quality Parameters Value Object
**File:** `src/AquaControl.Domain/ValueObjects/WaterQualityParameters.cs`
```csharp
using AquaControl.Domain.Common;

namespace AquaControl.Domain.ValueObjects;

public sealed class WaterQualityParameters : ValueObject
{
    public decimal? OptimalTemperature { get; }
    public decimal? MinTemperature { get; }
    public decimal? MaxTemperature { get; }
    public decimal? OptimalPH { get; }
    public decimal? MinPH { get; }
    public decimal? MaxPH { get; }
    public decimal? OptimalOxygen { get; }
    public decimal? MinOxygen { get; }
    public decimal? OptimalSalinity { get; }
    public decimal? MinSalinity { get; }
    public decimal? MaxSalinity { get; }

    private WaterQualityParameters(
        decimal? optimalTemperature, decimal? minTemperature, decimal? maxTemperature,
        decimal? optimalPH, decimal? minPH, decimal? maxPH,
        decimal? optimalOxygen, decimal? minOxygen,
        decimal? optimalSalinity, decimal? minSalinity, decimal? maxSalinity)
    {
        OptimalTemperature = optimalTemperature;
        MinTemperature = minTemperature;
        MaxTemperature = maxTemperature;
        OptimalPH = optimalPH;
        MinPH = minPH;
        MaxPH = maxPH;
        OptimalOxygen = optimalOxygen;
        MinOxygen = minOxygen;
        OptimalSalinity = optimalSalinity;
        MinSalinity = minSalinity;
        MaxSalinity = maxSalinity;
    }

    public static WaterQualityParameters Create(
        decimal? optimalTemperature = null, decimal? minTemperature = null, decimal? maxTemperature = null,
        decimal? optimalPH = null, decimal? minPH = null, decimal? maxPH = null,
        decimal? optimalOxygen = null, decimal? minOxygen = null,
        decimal? optimalSalinity = null, decimal? minSalinity = null, decimal? maxSalinity = null)
    {
        // Validation logic
        if (minTemperature.HasValue && maxTemperature.HasValue && minTemperature >= maxTemperature)
            throw new ArgumentException("Min temperature must be less than max temperature");

        if (minPH.HasValue && maxPH.HasValue && minPH >= maxPH)
            throw new ArgumentException("Min pH must be less than max pH");

        if (minSalinity.HasValue && maxSalinity.HasValue && minSalinity >= maxSalinity)
            throw new ArgumentException("Min salinity must be less than max salinity");

        return new WaterQualityParameters(
            optimalTemperature, minTemperature, maxTemperature,
            optimalPH, minPH, maxPH,
            optimalOxygen, minOxygen,
            optimalSalinity, minSalinity, maxSalinity);
    }

    public bool IsTemperatureInRange(decimal temperature)
    {
        if (MinTemperature.HasValue && temperature < MinTemperature.Value) return false;
        if (MaxTemperature.HasValue && temperature > MaxTemperature.Value) return false;
        return true;
    }

    public bool IsPHInRange(decimal ph)
    {
        if (MinPH.HasValue && ph < MinPH.Value) return false;
        if (MaxPH.HasValue && ph > MaxPH.Value) return false;
        return true;
    }

    public bool IsOxygenSufficient(decimal oxygen)
    {
        return !MinOxygen.HasValue || oxygen >= MinOxygen.Value;
    }

    public bool IsSalinityInRange(decimal salinity)
    {
        if (MinSalinity.HasValue && salinity < MinSalinity.Value) return false;
        if (MaxSalinity.HasValue && salinity > MaxSalinity.Value) return false;
        return true;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return OptimalTemperature ?? 0;
        yield return MinTemperature ?? 0;
        yield return MaxTemperature ?? 0;
        yield return OptimalPH ?? 0;
        yield return MinPH ?? 0;
        yield return MaxPH ?? 0;
        yield return OptimalOxygen ?? 0;
        yield return MinOxygen ?? 0;
        yield return OptimalSalinity ?? 0;
        yield return MinSalinity ?? 0;
        yield return MaxSalinity ?? 0;
    }
}
```

### File 12: Domain Project File
**File:** `src/AquaControl.Domain/AquaControl.Domain.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsAsErrors />
    <WarningsNotAsErrors>CS1591</WarningsNotAsErrors>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MediatR.Contracts" Version="2.0.1" />
  </ItemGroup>

</Project>
```

This completes the sophisticated Domain Layer with:

✅ **Domain-Driven Design (DDD)** - Proper aggregates, entities, value objects  
✅ **Rich Domain Models** - Business logic encapsulated in domain objects  
✅ **Domain Events** - Event-driven architecture foundation  
✅ **Value Objects** - Immutable, validated business concepts  
✅ **Aggregate Boundaries** - Proper encapsulation and consistency  
✅ **Business Rules** - Domain logic enforced at the model level  

Next, I'll create the Application Layer with CQRS, Command/Query handlers, and advanced patterns.
