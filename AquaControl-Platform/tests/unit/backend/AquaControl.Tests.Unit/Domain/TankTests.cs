using FluentAssertions;
using AquaControl.Domain.Aggregates.TankAggregate;
using AquaControl.Domain.ValueObjects;
using AquaControl.Domain.Enums;
using AquaControl.Domain.Events;

namespace AquaControl.Tests.Unit.Domain;

public class TankTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateTank()
    {
        // Arrange
        var name = "Test Tank";
        var capacity = TankCapacity.Create(1000, "L");
        var location = Location.Create("Building A", "Room 1");
        var tankType = TankType.Freshwater;

        // Act
        var tank = Tank.Create(name, capacity, location, tankType);

        // Assert
        tank.Should().NotBeNull();
        tank.Name.Should().Be(name);
        tank.Capacity.Should().Be(capacity);
        tank.Location.Should().Be(location);
        tank.TankType.Should().Be(tankType);
        tank.Status.Should().Be(TankStatus.Inactive);
        tank.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TankCreatedEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var capacity = TankCapacity.Create(1000, "L");
        var location = Location.Create("Building A", "Room 1");
        var tankType = TankType.Freshwater;

        // Act & Assert
        var act = () => Tank.Create(invalidName!, capacity, location, tankType);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Tank name cannot be empty*");
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateNameAndRaiseDomainEvent()
    {
        // Arrange
        var tank = CreateValidTank();
        var newName = "Updated Tank Name";
        tank.ClearDomainEvents(); // Clear creation event

        // Act
        tank.UpdateName(newName);

        // Assert
        tank.Name.Should().Be(newName);
        tank.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TankNameChangedEvent>()
            .Which.NewName.Should().Be(newName);
    }

    [Fact]
    public void UpdateName_WithSameName_ShouldNotRaiseDomainEvent()
    {
        // Arrange
        var tank = CreateValidTank();
        var currentName = tank.Name;
        tank.ClearDomainEvents(); // Clear creation event

        // Act
        tank.UpdateName(currentName);

        // Assert
        tank.Name.Should().Be(currentName);
        tank.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Activate_WithActiveSensors_ShouldActivateTank()
    {
        // Arrange
        var tank = CreateValidTank();
        var sensor = CreateValidSensor();
        tank.AddSensor(sensor);
        tank.ClearDomainEvents(); // Clear previous events

        // Act
        tank.Activate();

        // Assert
        tank.Status.Should().Be(TankStatus.Active);
        tank.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TankActivatedEvent>();
    }

    [Fact]
    public void Activate_WithoutActiveSensors_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var tank = CreateValidTank();

        // Act & Assert
        var act = () => tank.Activate();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Tank must have at least one active sensor to be activated");
    }

    [Fact]
    public void AddSensor_WithValidSensor_ShouldAddSensorAndRaiseDomainEvent()
    {
        // Arrange
        var tank = CreateValidTank();
        var sensor = CreateValidSensor();
        tank.ClearDomainEvents(); // Clear creation event

        // Act
        tank.AddSensor(sensor);

        // Assert
        tank.Sensors.Should().ContainSingle().Which.Should().Be(sensor);
        tank.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SensorAddedToTankEvent>();
    }

    [Fact]
    public void AddSensor_WhenMaxSensorsReached_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var tank = CreateValidTank();
        
        // Add maximum number of sensors (10)
        for (int i = 0; i < 10; i++)
        {
            tank.AddSensor(CreateValidSensor());
        }

        var extraSensor = CreateValidSensor();

        // Act & Assert
        var act = () => tank.AddSensor(extraSensor);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Tank cannot have more than 10 sensors");
    }

    [Fact]
    public void RemoveSensor_WithExistingSensor_ShouldRemoveSensorAndRaiseDomainEvent()
    {
        // Arrange
        var tank = CreateValidTank();
        var sensor = CreateValidSensor();
        tank.AddSensor(sensor);
        tank.ClearDomainEvents(); // Clear previous events

        // Act
        tank.RemoveSensor(sensor.Id);

        // Assert
        tank.Sensors.Should().BeEmpty();
        tank.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<SensorRemovedFromTankEvent>();
    }

    [Fact]
    public void IsMaintenanceDue_WhenMaintenanceDatePassed_ShouldReturnTrue()
    {
        // Arrange
        var tank = CreateValidTank();
        tank.ScheduleMaintenance(DateTime.UtcNow.AddDays(-1)); // Past date

        // Act
        var result = tank.IsMaintenanceDue();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsMaintenanceDue_WhenMaintenanceDateFuture_ShouldReturnFalse()
    {
        // Arrange
        var tank = CreateValidTank();
        tank.ScheduleMaintenance(DateTime.UtcNow.AddDays(1)); // Future date

        // Act
        var result = tank.IsMaintenanceDue();

        // Assert
        result.Should().BeFalse();
    }

    private static Tank CreateValidTank()
    {
        var capacity = TankCapacity.Create(1000, "L");
        var location = Location.Create("Building A", "Room 1");
        return Tank.Create("Test Tank", capacity, location, TankType.Freshwater);
    }

    private static Sensor CreateValidSensor()
    {
        return Sensor.Create(
            SensorType.Temperature,
            "Test Model",
            "Test Manufacturer",
            Guid.NewGuid().ToString(),
            95.0m);
    }
}

