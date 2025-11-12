using Microsoft.EntityFrameworkCore;
using AquaControl.Infrastructure.TimeSeries.Models;

namespace AquaControl.Infrastructure.TimeSeries;

/// <summary>
/// Database context for time-series data using TimescaleDB.
/// This context is separate from ReadModelDbContext to optimize for time-series queries.
/// </summary>
public sealed class TimeSeriesDbContext : DbContext
{
    public TimeSeriesDbContext(DbContextOptions<TimeSeriesDbContext> options) : base(options) { }

    public DbSet<SensorReading> SensorReadings { get; set; } = null!;
    public DbSet<Alert> Alerts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure SensorReading entity
        modelBuilder.Entity<SensorReading>(entity =>
        {
            entity.ToTable("SensorReadings", "timeseries");

            entity.HasKey(sr => sr.Id);

            entity.Property(sr => sr.Id)
                .ValueGeneratedNever();

            entity.Property(sr => sr.SensorId)
                .IsRequired();

            entity.Property(sr => sr.TankId)
                .IsRequired();

            entity.Property(sr => sr.Timestamp)
                .IsRequired();

            entity.Property(sr => sr.Value)
                .HasPrecision(10, 4)
                .IsRequired();

            entity.Property(sr => sr.QualityScore)
                .HasPrecision(3, 2)
                .IsRequired();

            entity.Property(sr => sr.Metadata)
                .HasColumnType("jsonb");

            entity.Property(sr => sr.CreatedAt)
                .IsRequired();

            // Indexes for time-series queries
            entity.HasIndex(sr => new { sr.Timestamp, sr.SensorId })
                .HasDatabaseName("IX_SensorReadings_Timestamp_SensorId");

            entity.HasIndex(sr => new { sr.TankId, sr.Timestamp })
                .HasDatabaseName("IX_SensorReadings_TankId_Timestamp");

            entity.HasIndex(sr => sr.Timestamp)
                .HasDatabaseName("IX_SensorReadings_Timestamp");
        });

        // Configure Alert entity
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.ToTable("Alerts", "timeseries");

            entity.HasKey(a => a.Id);

            entity.Property(a => a.Id)
                .ValueGeneratedNever();

            entity.Property(a => a.TankId)
                .IsRequired();

            entity.Property(a => a.AlertType)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(a => a.Severity)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(a => a.Message)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(a => a.ThresholdValue)
                .HasPrecision(10, 4);

            entity.Property(a => a.ActualValue)
                .HasPrecision(10, 4);

            entity.Property(a => a.ResolvedBy)
                .HasMaxLength(100);

            entity.Property(a => a.Metadata)
                .HasColumnType("jsonb");

            entity.Property(a => a.CreatedAt)
                .IsRequired();

            // Indexes for time-series queries
            entity.HasIndex(a => new { a.CreatedAt, a.TankId })
                .HasDatabaseName("IX_Alerts_CreatedAt_TankId");

            entity.HasIndex(a => a.IsResolved)
                .HasDatabaseName("IX_Alerts_IsResolved");

            entity.HasIndex(a => a.Severity)
                .HasDatabaseName("IX_Alerts_Severity");
        });
    }
}

