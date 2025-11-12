namespace AquaControl.Infrastructure.EventStore.Models;

public sealed class StoredEvent
{
    public Guid Id { get; set; }
    public Guid AggregateId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public string Metadata { get; set; } = string.Empty;
    public long Version { get; set; }
    public DateTime Timestamp { get; set; }
}

public sealed class EventSnapshot
{
    public Guid Id { get; set; }
    public Guid AggregateId { get; set; }
    public string AggregateType { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public long Version { get; set; }
    public DateTime Timestamp { get; set; }
}

