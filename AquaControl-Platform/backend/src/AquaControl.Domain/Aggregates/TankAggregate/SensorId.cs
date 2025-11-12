using AquaControl.Domain.Common;

namespace AquaControl.Domain.Aggregates.TankAggregate;

/// <summary>
/// Represents a strongly-typed identifier for a Sensor entity.
/// This value object wraps a <see cref="Guid"/> to provide type safety and prevent ID misuse.
/// </summary>
/// <remarks>
/// <para>
/// Strongly-typed IDs prevent common programming errors such as:
/// - Passing a TankId where a SensorId is expected
/// - Accidentally mixing different ID types
/// - Losing type information when passing IDs between methods
/// </para>
/// <para>
/// This class implements implicit conversion operators to allow seamless conversion
/// between <see cref="SensorId"/> and <see cref="Guid"/>, making it easy to use with
/// Entity Framework Core and other frameworks that expect Guid types.
/// </para>
/// </remarks>
public sealed class SensorId : ValueObject
{
    /// <summary>
    /// Gets the underlying GUID value of this sensor identifier.
    /// </summary>
    /// <value>A globally unique identifier.</value>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SensorId"/> class with the specified GUID value.
    /// </summary>
    /// <param name="value">The GUID value for this sensor identifier.</param>
    /// <remarks>
    /// This constructor is private to enforce the use of factory methods for creating instances.
    /// </remarks>
    private SensorId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="SensorId"/> with a randomly generated GUID.
    /// </summary>
    /// <returns>A new <see cref="SensorId"/> instance with a unique GUID.</returns>
    /// <remarks>
    /// Use this method when creating a new sensor and you need a unique identifier.
    /// </remarks>
    public static SensorId Create() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a new <see cref="SensorId"/> from an existing GUID value.
    /// </summary>
    /// <param name="value">The GUID value to use for the sensor identifier.</param>
    /// <returns>A new <see cref="SensorId"/> instance with the specified GUID.</returns>
    /// <remarks>
    /// Use this method when reconstructing a SensorId from a stored GUID value,
    /// such as when loading from a database or deserializing from JSON.
    /// </remarks>
    public static SensorId Create(Guid value) => new(value);

    /// <summary>
    /// Implicitly converts a <see cref="SensorId"/> to a <see cref="Guid"/>.
    /// </summary>
    /// <param name="sensorId">The sensor identifier to convert.</param>
    /// <returns>The underlying GUID value.</returns>
    /// <remarks>
    /// This operator allows SensorId to be used seamlessly with APIs that expect Guid,
    /// such as Entity Framework Core, database operations, and serialization.
    /// </remarks>
    public static implicit operator Guid(SensorId sensorId) => sensorId.Value;

    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="SensorId"/>.
    /// </summary>
    /// <param name="value">The GUID value to convert.</param>
    /// <returns>A new <see cref="SensorId"/> instance.</returns>
    /// <remarks>
    /// This operator allows Guid values to be automatically converted to SensorId,
    /// making it convenient to work with existing code that uses Guid types.
    /// </remarks>
    public static implicit operator SensorId(Guid value) => new(value);

    /// <summary>
    /// Returns a string representation of this sensor identifier.
    /// </summary>
    /// <returns>The string representation of the underlying GUID value.</returns>
    public override string ToString() => Value.ToString();

    /// <summary>
    /// Gets the components that should be used for equality comparison.
    /// </summary>
    /// <returns>An enumerable containing the GUID value.</returns>
    /// <remarks>
    /// Two SensorId instances are considered equal if their underlying GUID values are equal.
    /// </remarks>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

