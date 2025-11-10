namespace AquaControl.Domain.Common;

/// <summary>
/// Base class for aggregate roots in the domain model.
/// Implements Domain-Driven Design (DDD) aggregate pattern with event sourcing support.
/// </summary>
/// <remarks>
/// <para>
/// An aggregate root is the entry point to an aggregate - a cluster of domain objects
/// that are treated as a single unit for data changes. This class provides:
/// </para>
/// <list type="bullet">
/// <item><description>Version tracking for optimistic concurrency control</description></item>
/// <item><description>Audit fields (CreatedAt, UpdatedAt) for tracking entity lifecycle</description></item>
/// <item><description>Domain event application mechanism for event sourcing</description></item>
/// </list>
/// <para>
/// All domain aggregates should inherit from this class rather than Entity directly.
/// </para>
/// </remarks>
/// <typeparam name="TId">The type of the aggregate's unique identifier. Must be a non-nullable type.</typeparam>
/// <example>
/// <code>
/// public sealed class Tank : AggregateRoot&lt;TankId&gt;
/// {
///     private Tank(TankId id) : base(id) { }
///     
///     public static Tank Create(string name, TankCapacity capacity)
///     {
///         var tankId = TankId.Create();
///         var tank = new Tank(tankId);
///         tank.Apply(new TankCreatedEvent(tankId, name, capacity));
///         return tank;
///     }
/// }
/// </code>
/// </example>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the aggregate root.</param>
    protected AggregateRoot(TId id) : base(id) { }

    /// <summary>
    /// Parameterless constructor required by Entity Framework Core for materialization.
    /// Should not be used directly in domain code - use the parameterized constructor instead.
    /// </summary>
    protected AggregateRoot() { } // For EF Core

    /// <summary>
    /// Gets or sets the UTC timestamp when the aggregate was first created.
    /// </summary>
    /// <value>
    /// The creation timestamp. Automatically set to <see cref="DateTime.UtcNow"/> when the aggregate is instantiated.
    /// </value>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the UTC timestamp when the aggregate was last modified.
    /// </summary>
    /// <value>
    /// The last update timestamp. Automatically updated when <see cref="IncrementVersion"/> is called.
    /// Returns <c>null</c> if the aggregate has never been modified.
    /// </value>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Gets or sets the version number of the aggregate for optimistic concurrency control.
    /// </summary>
    /// <value>
    /// The version number, starting at 0. Incremented each time a domain event is applied
    /// via the <see cref="Apply"/> method. Used to detect concurrent modification conflicts.
    /// </value>
    /// <remarks>
    /// This version is used in event sourcing and optimistic concurrency scenarios.
    /// When saving changes, the repository should check that the version matches the expected value.
    /// </remarks>
    public long Version { get; protected set; } = 0;

    /// <summary>
    /// Increments the aggregate version and updates the <see cref="UpdatedAt"/> timestamp.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is called automatically when applying domain events via <see cref="Apply"/>.
    /// It should not be called directly unless implementing custom event application logic.
    /// </para>
    /// <para>
    /// The version increment is essential for:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Optimistic concurrency control - detecting concurrent modifications</description></item>
    /// <item><description>Event sourcing - tracking the sequence of events applied to the aggregate</description></item>
    /// <item><description>Snapshot creation - determining when to create aggregate snapshots</description></item>
    /// </list>
    /// </remarks>
    protected void IncrementVersion()
    {
        Version++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Applies a domain event to the aggregate, adding it to the domain events collection
    /// and incrementing the version.
    /// </summary>
    /// <param name="domainEvent">The domain event to apply. Must not be <c>null</c>.</param>
    /// <remarks>
    /// <para>
    /// This method encapsulates the standard pattern for applying domain events:
    /// </para>
    /// <list type="number">
    /// <item><description>Add the event to the domain events collection (for later persistence)</description></item>
    /// <item><description>Increment the aggregate version (for concurrency control)</description></item>
    /// </list>
    /// <para>
    /// Domain events are typically published after the aggregate is persisted to the event store.
    /// The events are collected in the <see cref="Entity{TId}.DomainEvents"/> collection and
    /// cleared after successful persistence.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public void UpdateName(string newName)
    /// {
    ///     if (Name == newName) return;
    ///     
    ///     var oldName = Name;
    ///     Name = newName;
    ///     
    ///     // Apply domain event - automatically increments version
    ///     Apply(new TankNameChangedEvent(Id, oldName, newName));
    /// }
    /// </code>
    /// </example>
    protected void Apply(IDomainEvent domainEvent)
    {
        AddDomainEvent(domainEvent);
        IncrementVersion();
    }
}