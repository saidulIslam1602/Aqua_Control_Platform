using MediatR;

namespace AquaControl.Domain.Common;

/// <summary>
/// Represents a domain event - something significant that happened in the domain.
/// Domain events are raised by entities when important state changes occur.
/// </summary>
/// <remarks>
/// This interface extends <see cref="INotification"/> from MediatR, enabling event-driven architecture.
/// Domain events are published after entities are successfully persisted, allowing other parts of the system
/// to react to domain occurrences without tight coupling.
/// </remarks>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Gets the unique identifier for this domain event.
    /// </summary>
    /// <value>A globally unique identifier (GUID) that uniquely identifies this event instance.</value>
    /// <remarks>
    /// Each event instance has a unique ID, even if multiple events of the same type are raised.
    /// This enables event deduplication, tracking, and correlation.
    /// </remarks>
    Guid EventId { get; }

    /// <summary>
    /// Gets the date and time when this domain event occurred.
    /// </summary>
    /// <value>The UTC timestamp when the event was raised.</value>
    /// <remarks>
    /// Uses UTC timezone to ensure consistency across different server locations and timezones.
    /// This timestamp represents when the event was created, not when it was published.
    /// </remarks>
    DateTime OccurredOn { get; }

    /// <summary>
    /// Gets the type name of this domain event.
    /// </summary>
    /// <value>A string representing the event type (e.g., "TankCreated", "WaterLevelLow").</value>
    /// <remarks>
    /// Used for event serialization, deserialization, and routing to appropriate handlers.
    /// Typically matches the class name of the concrete event implementation.
    /// </remarks>
    string EventType { get; }
}

/// <summary>
/// Base implementation of <see cref="IDomainEvent"/> that provides default behavior for common properties.
/// Concrete domain events should inherit from this record type.
/// </summary>
/// <remarks>
/// This abstract record provides:
/// - Automatic generation of unique event IDs
/// - Automatic timestamping with UTC time
/// - Requires derived classes to specify the event type name
/// 
/// Using a record type provides value-based equality and immutability by default.
/// </remarks>
/// <example>
/// <code>
/// public sealed record TankCreatedEvent(Guid TankId, string Name) : DomainEvent
/// {
///     public override string EventType => nameof(TankCreatedEvent);
/// }
/// </code>
/// </example>
public abstract record DomainEvent : IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier for this domain event.
    /// Automatically generated when the event is created.
    /// </summary>
    /// <value>A new GUID generated for each event instance.</value>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the date and time when this domain event occurred.
    /// Automatically set to the current UTC time when the event is created.
    /// </summary>
    /// <value>The UTC timestamp when the event instance was created.</value>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the type name of this domain event.
    /// Must be implemented by derived classes to return the event type name.
    /// </summary>
    /// <value>A string representing the event type name.</value>
    /// <remarks>
    /// Typically implemented using <c>nameof(EventClassName)</c> to ensure type safety.
    /// </remarks>
    public abstract string EventType { get; }
}