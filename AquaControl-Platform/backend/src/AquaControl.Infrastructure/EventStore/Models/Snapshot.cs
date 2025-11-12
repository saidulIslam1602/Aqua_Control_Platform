namespace AquaControl.Infrastructure.EventStore.Models;

public sealed class Snapshot
{
    public Guid Id { get; set; }
    public Guid AggregateId { get; set; }
    public string AggregateType { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public long Version { get; set; }
    public DateTime Timestamp { get; set; }
}
