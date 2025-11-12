using Microsoft.EntityFrameworkCore;
using AquaControl.Infrastructure.EventStore.Models;

namespace AquaControl.Infrastructure.EventStore;

public sealed class EventStoreDbContext : DbContext
{
    public EventStoreDbContext(DbContextOptions<EventStoreDbContext> options) : base(options) { }

    public DbSet<StoredEvent> Events { get; set; } = null!;
    public DbSet<EventSnapshot> Snapshots { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StoredEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AggregateId).IsRequired();
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(255);
            entity.Property(e => e.EventData).IsRequired();
            entity.Property(e => e.Metadata).IsRequired();
            entity.Property(e => e.Version).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();

            // Indexes for performance
            entity.HasIndex(e => e.AggregateId);
            entity.HasIndex(e => new { e.AggregateId, e.Version }).IsUnique();
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.EventType);

            // Table name
            entity.ToTable("Events", "EventStore");
        });

        modelBuilder.Entity<EventSnapshot>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.AggregateId).IsRequired();
            entity.Property(s => s.AggregateType).IsRequired().HasMaxLength(255);
            entity.Property(s => s.Data).IsRequired();
            entity.Property(s => s.Version).IsRequired();
            entity.Property(s => s.Timestamp).IsRequired();

            // Indexes
            entity.HasIndex(s => s.AggregateId).IsUnique();
            entity.HasIndex(s => s.Timestamp);

            // Table name
            entity.ToTable("Snapshots", "EventStore");
        });
    }
}

