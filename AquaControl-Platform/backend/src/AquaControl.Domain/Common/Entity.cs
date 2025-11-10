using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace AquaControl.Domain.Common;

/// <summary>
/// Base class for all domain entities in the AquaCulture Platform.
/// Provides common functionality for entity identity, equality comparison, and domain event tracking.
/// </summary>
/// <typeparam name="TId">The type of the entity's unique identifier. Must be a non-nullable type.</typeparam>
/// <remarks>
/// This abstract class implements the IEquatable interface for type-safe equality comparison.
/// All domain entities should inherit from this class to ensure consistent behavior across the domain model.
/// Domain events are tracked internally and can be published after entity persistence.
/// </remarks>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>
    /// Internal collection of domain events raised by this entity.
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = new();
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for this entity.</param>
    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class.
    /// This parameterless constructor is required by Entity Framework Core for materialization.
    /// </summary>
    protected Entity() { }

    /// <summary>
    /// Gets or sets the unique identifier for this entity.
    /// </summary>
    /// <value>The unique identifier of type <typeparamref name="TId"/>.</value>
    public TId Id { get; protected set; } = default!;

    /// <summary>
    /// Gets the collection of domain events raised by this entity.
    /// This property is not mapped to the database.
    /// </summary>
    /// <value>A read-only list of domain events.</value>
    /// <remarks>
    /// Domain events are typically published after the entity is successfully persisted.
    /// Use <see cref="AddDomainEvent(IDomainEvent)"/> to add events, and <see cref="ClearDomainEvents"/> after publishing.
    /// </remarks>
    [NotMapped]
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to this entity's event collection.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    /// <remarks>
    /// Domain events represent something significant that happened in the domain.
    /// They are typically published after the entity is successfully persisted to the database.
    /// </remarks>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a specific domain event from this entity's event collection.
    /// </summary>
    /// <param name="domainEvent">The domain event to remove.</param>
    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clears all domain events from this entity's event collection.
    /// This method should be called after domain events have been published.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Determines whether the current entity is equal to another entity of the same type.
    /// </summary>
    /// <param name="other">An entity to compare with this entity.</param>
    /// <returns>
    /// <c>true</c> if the current entity is equal to the <paramref name="other"/> parameter;
    /// otherwise, <c>false</c>. Entities are considered equal if their IDs are equal.
    /// </returns>
    /// <remarks>
    /// This method implements the <see cref="IEquatable{T}.Equals(T)"/> interface.
    /// Two entities are considered equal if they have the same ID, regardless of other properties.
    /// </remarks>
    public bool Equals(Entity<TId>? other)
    {
        return other is not null && Id.Equals(other.Id);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current entity.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns>
    /// <c>true</c> if the specified object is an <see cref="Entity{TId}"/> and is equal to the current entity;
    /// otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> entity && Equals(entity);
    }

    /// <summary>
    /// Serves as the default hash function for the entity.
    /// </summary>
    /// <returns>A hash code for the current entity based on its ID.</returns>
    /// <remarks>
    /// The hash code is derived from the entity's ID, ensuring that entities with the same ID
    /// produce the same hash code. This is important for proper behavior in hash-based collections.
    /// </remarks>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// Determines whether two entities are equal using the equality operator.
    /// </summary>
    /// <param name="left">The first entity to compare.</param>
    /// <param name="right">The second entity to compare.</param>
    /// <returns>
    /// <c>true</c> if the entities are equal; otherwise, <c>false</c>.
    /// </returns>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Determines whether two entities are not equal using the inequality operator.
    /// </summary>
    /// <param name="left">The first entity to compare.</param>
    /// <param name="right">The second entity to compare.</param>
    /// <returns>
    /// <c>true</c> if the entities are not equal; otherwise, <c>false</c>.
    /// </returns>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !Equals(left, right);
    }
}