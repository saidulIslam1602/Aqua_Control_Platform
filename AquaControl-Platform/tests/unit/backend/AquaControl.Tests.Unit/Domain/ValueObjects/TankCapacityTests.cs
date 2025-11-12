using FluentAssertions;
using AquaControl.Domain.ValueObjects;

namespace AquaControl.Tests.Unit.Domain.ValueObjects;

public class TankCapacityTests
{
    [Theory]
    [InlineData(1000, "L")]
    [InlineData(50.5, "ML")]
    [InlineData(264.172, "GAL")]
    public void Create_WithValidParameters_ShouldCreateTankCapacity(decimal value, string unit)
    {
        // Act
        var capacity = TankCapacity.Create(value, unit);

        // Assert
        capacity.Should().NotBeNull();
        capacity.Value.Should().Be(value);
        capacity.Unit.Should().Be(unit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.5)]
    public void Create_WithNonPositiveValue_ShouldThrowArgumentException(decimal invalidValue)
    {
        // Act & Assert
        var act = () => TankCapacity.Create(invalidValue, "L");
        act.Should().Throw<ArgumentException>()
            .WithMessage("Tank capacity must be positive*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidUnit_ShouldThrowArgumentException(string invalidUnit)
    {
        // Act & Assert
        var act = () => TankCapacity.Create(1000, invalidUnit!);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Unit cannot be empty*");
    }

    [Fact]
    public void ConvertTo_FromLitersToMilliliters_ShouldConvertCorrectly()
    {
        // Arrange
        var capacity = TankCapacity.Create(1, "L");

        // Act
        var converted = capacity.ConvertTo("ML");

        // Assert
        converted.Value.Should().Be(1000);
        converted.Unit.Should().Be("ML");
    }

    [Fact]
    public void ConvertTo_FromMillilitersToLiters_ShouldConvertCorrectly()
    {
        // Arrange
        var capacity = TankCapacity.Create(1000, "ML");

        // Act
        var converted = capacity.ConvertTo("L");

        // Assert
        converted.Value.Should().Be(1);
        converted.Unit.Should().Be("L");
    }

    [Fact]
    public void ConvertTo_FromLitersToGallons_ShouldConvertCorrectly()
    {
        // Arrange
        var capacity = TankCapacity.Create(1, "L");

        // Act
        var converted = capacity.ConvertTo("GAL");

        // Assert
        converted.Value.Should().BeApproximately(0.264172m, 0.000001m);
        converted.Unit.Should().Be("GAL");
    }

    [Fact]
    public void ConvertTo_ToSameUnit_ShouldReturnSameValue()
    {
        // Arrange
        var capacity = TankCapacity.Create(1000, "L");

        // Act
        var converted = capacity.ConvertTo("L");

        // Assert
        converted.Value.Should().Be(1000);
        converted.Unit.Should().Be("L");
    }

    [Fact]
    public void ConvertTo_WithUnsupportedConversion_ShouldThrowArgumentException()
    {
        // Arrange
        var capacity = TankCapacity.Create(1000, "L");

        // Act & Assert
        var act = () => capacity.ConvertTo("INVALID");
        act.Should().Throw<ArgumentException>()
            .WithMessage("Cannot convert from L to INVALID");
    }

    [Fact]
    public void Equals_WithSameValueAndUnit_ShouldReturnTrue()
    {
        // Arrange
        var capacity1 = TankCapacity.Create(1000, "L");
        var capacity2 = TankCapacity.Create(1000, "L");

        // Act & Assert
        capacity1.Should().Be(capacity2);
        capacity1.Equals(capacity2).Should().BeTrue();
        (capacity1 == capacity2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var capacity1 = TankCapacity.Create(1000, "L");
        var capacity2 = TankCapacity.Create(2000, "L");

        // Act & Assert
        capacity1.Should().NotBe(capacity2);
        capacity1.Equals(capacity2).Should().BeFalse();
        (capacity1 == capacity2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentUnits_ShouldReturnFalse()
    {
        // Arrange
        var capacity1 = TankCapacity.Create(1000, "L");
        var capacity2 = TankCapacity.Create(1000, "ML");

        // Act & Assert
        capacity1.Should().NotBe(capacity2);
        capacity1.Equals(capacity2).Should().BeFalse();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var capacity = TankCapacity.Create(1000.50m, "L");

        // Act
        var result = capacity.ToString();

        // Assert
        result.Should().Be("1000.50 L");
    }
}

