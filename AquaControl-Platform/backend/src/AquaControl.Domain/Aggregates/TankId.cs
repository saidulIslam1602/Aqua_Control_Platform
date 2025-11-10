using AquaControl.Domain.Common;

namespace AquaControl.Domain.Aggregates.TankAggregate;

/// <summary>
/// Represents a strongly-typed identifier for a Tank aggregate.
/// This value object wraps a <see cref="Guid"/> to provide type safety and prevent ID misuse.
/// </summary>
/// <remarks>
/// <para>
/// Strongly-typed IDs prevent common programming errors such as:
/// - Passing a SensorId where a TankId is expected
/// - Accidentally mixing different ID types
/// - Losing type information when passing IDs between methods
/// </para>
/// <para>
/// This class implements implicit conversion operators to allow seamless conversion
/// between <see cref="TankId"/> and <see cref="Guid"/>, making it easy to use with
/// Entity Framework Core and other frameworks that expect Guid types.
/// </para>
/// </remarks>
public sealed class TankId : ValueObject
{
    /// <summary>
    /// Gets the underlying GUID value of this tank identifier.
    /// </summary>
    /// <value>A globally unique identifier.</value>
    public Guid Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TankId"/> class with the specified GUID value.
    /// </summary>
    /// <param name="value">The GUID value for this tank identifier.</param>
    /// <remarks>
    /// This constructor is private to enforce the use of factory methods for creating instances.
    /// </remarks>
    private TankId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="TankId"/> with a randomly generated GUID.
    /// </summary>
    /// <returns>A new <see cref="TankId"/> instance with a unique GUID.</returns>
    /// <remarks>
    /// Use this method when creating a new tank and you need a unique identifier.
    /// </remarks>
    public static TankId Create() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a new <see cref="TankId"/> from an existing GUID value.
    /// </summary>
    /// <param name="value">The GUID value to use for the tank identifier.</param>
    /// <returns>A new <see cref="TankId"/> instance with the specified GUID.</returns>
    /// <remarks>
    /// Use this method when reconstructing a TankId from a stored GUID value,
    /// such as when loading from a database or deserializing from JSON.
    /// </remarks>
    public static TankId Create(Guid value) => new(value);

    /// <summary>
    /// Implicitly converts a <see cref="TankId"/> to a <see cref="Guid"/>.
    /// </summary>
    /// <param name="tankId">The tank identifier to convert.</param>
    /// <returns>The underlying GUID value.</returns>
    /// <remarks>
    /// This operator allows TankId to be used seamlessly with APIs that expect Guid,
    /// such as Entity Framework Core, database operations, and serialization.
    /// </remarks>
    public static implicit operator Guid(TankId tankId) => tankId.Value;

    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="TankId"/>.
    /// </summary>
    /// <param name="value">The GUID value to convert.</param>
    /// <returns>A new <see cref="TankId"/> instance.</returns>
    /// <remarks>
    /// This operator allows Guid values to be automatically converted to TankId,
    /// making it convenient to work with existing code that uses Guid types.
    /// </remarks>
    public static implicit operator TankId(Guid value) => new(value);

    /// <summary>
    /// Returns a string representation of this tank identifier.
    /// </summary>
    /// <returns>The string representation of the underlying GUID value.</returns>
    public override string ToString() => Value.ToString();

    /// <summary>
    /// Gets the components that should be used for equality comparison.
    /// </summary>
    /// <returns>An enumerable containing the GUID value.</returns>
    /// <remarks>
    /// Two TankId instances are considered equal if their underlying GUID values are equal.
    /// </remarks>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}