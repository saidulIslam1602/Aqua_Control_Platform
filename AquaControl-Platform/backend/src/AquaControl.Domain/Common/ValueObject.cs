using System.Linq;

namespace AquaControl.Domain.Common;

/// <summary>
/// Base class for all value objects in the AquaCulture Platform domain.
/// Value objects are immutable objects that are defined by their attributes rather than identity.
/// </summary>
/// <remarks>
/// <para>
/// Value objects differ from entities in that they:
/// - Have no identity (compared by all property values)
/// - Are immutable (cannot be changed after creation)
/// - Represent descriptive aspects of the domain
/// </para>
/// <para>
/// Derived classes must implement <see cref="GetEqualityComponents"/> to specify which properties
/// should be used for equality comparison. All specified properties must have equal values for
/// two value objects to be considered equal.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class Money : ValueObject
/// {
///     public decimal Amount { get; }
///     public string Currency { get; }
///     
///     public Money(decimal amount, string currency)
///     {
///         Amount = amount;
///         Currency = currency;
///     }
///     
///     protected override IEnumerable&lt;object&gt; GetEqualityComponents()
///     {
///         yield return Amount;
///         yield return Currency;
///     }
/// }
/// </code>
/// </example>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Gets the components that should be used for equality comparison.
    /// Derived classes must implement this method to return all properties that define equality.
    /// </summary>
    /// <returns>An enumerable of objects representing the equality components.</returns>
    /// <remarks>
    /// All properties returned by this method must have equal values for two value objects
    /// to be considered equal. The order of components matters for comparison.
    /// </remarks>
    protected abstract IEnumerable<object> GetEqualityComponents();

    /// <summary>
    /// Determines whether the specified object is equal to the current value object.
    /// </summary>
    /// <param name="obj">The object to compare with the current value object.</param>
    /// <returns>
    /// <c>true</c> if the specified object is a value object of the same type and all equality
    /// components are equal; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Two value objects are equal if they are of the same type and all components returned
    /// by <see cref="GetEqualityComponents"/> have equal values.
    /// </remarks>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Determines whether the current value object is equal to another value object.
    /// </summary>
    /// <param name="other">A value object to compare with this value object.</param>
    /// <returns>
    /// <c>true</c> if the current value object is equal to the <paramref name="other"/> parameter;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method implements the <see cref="IEquatable{T}.Equals(T)"/> interface for type-safe comparison.
    /// </remarks>
    public bool Equals(ValueObject? other)
    {
        return Equals((object?)other);
    }

    /// <summary>
    /// Serves as the default hash function for the value object.
    /// </summary>
    /// <returns>A hash code for the current value object based on its equality components.</returns>
    /// <remarks>
    /// The hash code is computed by combining the hash codes of all equality components using XOR.
    /// This ensures that value objects with equal components produce the same hash code, which is
    /// required for proper behavior in hash-based collections like Dictionary and HashSet.
    /// </remarks>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    /// <summary>
    /// Determines whether two value objects are equal using the equality operator.
    /// </summary>
    /// <param name="left">The first value object to compare.</param>
    /// <param name="right">The second value object to compare.</param>
    /// <returns>
    /// <c>true</c> if the value objects are equal; otherwise, <c>false</c>.
    /// </returns>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Determines whether two value objects are not equal using the inequality operator.
    /// </summary>
    /// <param name="left">The first value object to compare.</param>
    /// <param name="right">The second value object to compare.</param>
    /// <returns>
    /// <c>true</c> if the value objects are not equal; otherwise, <c>false</c>.
    /// </returns>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !Equals(left, right);
    }
}