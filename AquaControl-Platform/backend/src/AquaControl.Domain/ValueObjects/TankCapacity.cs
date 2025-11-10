using AquaControl.Domain.Common;

namespace AquaControl.Domain.ValueObjects;

/// <summary>
/// Represents the capacity of a tank with a value and unit of measurement.
/// This is a value object that encapsulates capacity information and provides unit conversion capabilities.
/// </summary>
/// <remarks>
/// <para>
/// TankCapacity is an immutable value object that represents a tank's capacity measurement.
/// It enforces business rules such as:
/// </para>
/// <list type="bullet">
/// <item><description>Capacity must be positive (greater than zero)</description></item>
/// <item><description>Unit must be a valid measurement unit (L, ML, or GAL)</description></item>
/// <item><description>Provides unit conversion between supported units</description></item>
/// </list>
/// <para>
/// Two TankCapacity instances are considered equal if they have the same value and unit
/// (after normalization). For example, 1000 ML equals 1 L.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Create a tank capacity
/// var capacity = TankCapacity.Create(1000, "L");
/// 
/// // Convert to different unit
/// var capacityInGallons = capacity.ConvertTo("GAL");
/// 
/// // Compare capacities
/// var capacity2 = TankCapacity.Create(1, "L");
/// bool areEqual = capacity.Equals(capacity2); // true if same value and unit
/// </code>
/// </example>
public sealed class TankCapacity : ValueObject
{
    /// <summary>
    /// Gets the numeric value of the tank capacity.
    /// </summary>
    /// <value>
    /// The capacity value. Always positive and greater than zero.
    /// </value>
    public decimal Value { get; }

    /// <summary>
    /// Gets the unit of measurement for the capacity value.
    /// </summary>
    /// <value>
    /// The unit of measurement. Supported values: "L" (Liters), "ML" (Milliliters), "GAL" (Gallons).
    /// </value>
    public string Unit { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TankCapacity"/> class.
    /// </summary>
    /// <param name="value">The numeric capacity value. Must be greater than zero.</param>
    /// <param name="unit">The unit of measurement. Must not be null or empty.</param>
    /// <remarks>
    /// This constructor is private to enforce the use of the <see cref="Create"/> factory method,
    /// which performs validation before creating instances.
    /// </remarks>
    private TankCapacity(decimal value, string unit)
    {
        Value = value;
        Unit = unit;
    }

    /// <summary>
    /// Creates a new instance of <see cref="TankCapacity"/> with validation.
    /// </summary>
    /// <param name="value">The numeric capacity value. Must be greater than zero.</param>
    /// <param name="unit">The unit of measurement. Defaults to "L" (Liters) if not specified.</param>
    /// <returns>A new <see cref="TankCapacity"/> instance with the specified value and unit.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when:
    /// <list type="bullet">
    /// <item><description><paramref name="value"/> is less than or equal to zero</description></item>
    /// <item><description><paramref name="unit"/> is null, empty, or whitespace</description></item>
    /// </list>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This factory method ensures that all TankCapacity instances are created with valid values.
    /// It performs validation before creating the instance, preventing invalid capacity objects
    /// from being created.
    /// </para>
    /// <para>
    /// The default unit is "L" (Liters), which is the standard unit for tank capacity measurements
    /// in the aquaculture domain.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create with default unit (Liters)
    /// var capacity1 = TankCapacity.Create(1000);
    /// 
    /// // Create with explicit unit
    /// var capacity2 = TankCapacity.Create(500, "ML");
    /// 
    /// // Invalid - will throw ArgumentException
    /// var capacity3 = TankCapacity.Create(-100, "L"); // Throws exception
    /// </code>
    /// </example>
    public static TankCapacity Create(decimal value, string unit = "L")
    {
        if (value <= 0)
            throw new ArgumentException("Tank capacity must be positive", nameof(value));

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Unit cannot be empty", nameof(unit));

        return new TankCapacity(value, unit);
    }

    /// <summary>
    /// Converts the tank capacity to a different unit of measurement.
    /// </summary>
    /// <param name="targetUnit">The target unit to convert to. Must be one of: "L", "ML", or "GAL".</param>
    /// <returns>
    /// A new <see cref="TankCapacity"/> instance with the converted value and the target unit.
    /// The original instance remains unchanged (immutability).
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when conversion between the current unit and <paramref name="targetUnit"/> is not supported.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Supported conversions:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Liters (L) ↔ Milliliters (ML): 1 L = 1000 ML</description></item>
    /// <item><description>Liters (L) ↔ Gallons (GAL): 1 L ≈ 0.264172 GAL</description></item>
    /// <item><description>Same unit: Returns a new instance with the same value</description></item>
    /// </list>
    /// <para>
    /// This method is case-insensitive for unit names. The conversion uses precise decimal arithmetic
    /// to maintain accuracy.
    /// </para>
    /// <para>
    /// Since value objects are immutable, this method returns a new instance rather than modifying
    /// the current one.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var capacity = TankCapacity.Create(1, "L");
    /// 
    /// // Convert to milliliters
    /// var inMilliliters = capacity.ConvertTo("ML"); // Returns 1000 ML
    /// 
    /// // Convert to gallons
    /// var inGallons = capacity.ConvertTo("GAL"); // Returns ~0.264 GAL
    /// 
    /// // Original remains unchanged
    /// Console.WriteLine(capacity); // Still prints "1.00 L"
    /// </code>
    /// </example>
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

    /// <summary>
    /// Returns a string representation of the tank capacity.
    /// </summary>
    /// <returns>
    /// A formatted string showing the capacity value (with 2 decimal places) and unit.
    /// Format: "{Value:F2} {Unit}" (e.g., "1000.00 L", "500.00 ML").
    /// </returns>
    /// <remarks>
    /// The value is formatted with 2 decimal places for readability while maintaining precision
    /// for display purposes. For exact precision, use the <see cref="Value"/> property directly.
    /// </remarks>
    /// <example>
    /// <code>
    /// var capacity = TankCapacity.Create(1000.5m, "L");
    /// Console.WriteLine(capacity); // Outputs: "1000.50 L"
    /// </code>
    /// </example>
    public override string ToString() => $"{Value:F2} {Unit}";

    /// <summary>
    /// Gets the components that define equality for this value object.
    /// </summary>
    /// <returns>
    /// An enumerable containing the <see cref="Value"/> and normalized <see cref="Unit"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Two TankCapacity instances are considered equal if:
    /// </para>
    /// <list type="bullet">
    /// <item><description>They have the same <see cref="Value"/></description></item>
    /// <item><description>They have the same <see cref="Unit"/> (case-insensitive comparison)</description></item>
    /// </list>
    /// <para>
    /// The unit is normalized to uppercase for comparison, so "L" and "l" are considered equal.
    /// However, note that 1000 ML and 1 L are NOT considered equal by this method - they have
    /// different values. Use <see cref="ConvertTo"/> to normalize units before comparison if needed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var capacity1 = TankCapacity.Create(1000, "L");
    /// var capacity2 = TankCapacity.Create(1000, "l"); // lowercase
    /// bool areEqual = capacity1.Equals(capacity2); // true (case-insensitive unit comparison)
    /// 
    /// var capacity3 = TankCapacity.Create(1000, "ML");
    /// bool areEqual2 = capacity1.Equals(capacity3); // false (different units, even though 1000 ML = 1 L)
    /// </code>
    /// </example>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Unit.ToUpperInvariant();
    }
}