using Microsoft.EntityFrameworkCore;
using AquaControl.Infrastructure.ReadModels.Models;

namespace AquaControl.Infrastructure.ReadModels;

public sealed class ReadModelDbContext : DbContext
{
    public ReadModelDbContext(DbContextOptions<ReadModelDbContext> options) : base(options) { }

    public DbSet<TankReadModel> Tanks { get; set; } = null!;
    public DbSet<SensorReadModel> Sensors { get; set; } = null!;
    public DbSet<SensorReadingReadModel> SensorReadings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TankReadModel>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
            entity.Property(t => t.TankType).IsRequired().HasMaxLength(50);
            entity.Property(t => t.Status).IsRequired().HasMaxLength(50);
            entity.Property(t => t.Building).IsRequired().HasMaxLength(100);
            entity.Property(t => t.Room).IsRequired().HasMaxLength(100);
            
            // Indexes for performance
            entity.HasIndex(t => t.Name).IsUnique();
            entity.HasIndex(t => t.Status);
            entity.HasIndex(t => t.TankType);
            entity.HasIndex(t => t.CreatedAt);

            entity.ToTable("Tanks", "ReadModel");
        });

        modelBuilder.Entity<SensorReadModel>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.SensorType).IsRequired().HasMaxLength(50);
            entity.Property(s => s.Status).IsRequired().HasMaxLength(50);
            entity.Property(s => s.Model).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Manufacturer).IsRequired().HasMaxLength(100);
            entity.Property(s => s.SerialNumber).IsRequired().HasMaxLength(100);

            // Foreign key to tank
            entity.HasOne<TankReadModel>()
                  .WithMany()
                  .HasForeignKey(s => s.TankId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(s => s.TankId);
            entity.HasIndex(s => s.SensorType);
            entity.HasIndex(s => s.Status);
            entity.HasIndex(s => s.SerialNumber).IsUnique();

            entity.ToTable("Sensors", "ReadModel");
        });

        modelBuilder.Entity<SensorReadingReadModel>(entity =>
        {
            entity.HasKey(sr => sr.Id);
            entity.Property(sr => sr.Value).HasPrecision(10, 4);
            entity.Property(sr => sr.QualityScore).HasPrecision(3, 2);

            // Foreign key to sensor
            entity.HasOne<SensorReadModel>()
                  .WithMany()
                  .HasForeignKey(sr => sr.SensorId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Indexes for time-series queries
            entity.HasIndex(sr => new { sr.SensorId, sr.Timestamp });
            entity.HasIndex(sr => sr.Timestamp);

            entity.ToTable("SensorReadings", "ReadModel");
        });
    }
}

