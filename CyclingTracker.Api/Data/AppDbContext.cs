using CyclingTracker.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CyclingTracker.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<Athlete, IdentityRole<Guid>, Guid>(options)
{

    public DbSet<Session> Sessions => Set<Session>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Session>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Sport)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Session>()
                .OwnsOne(s => s.Samples, b => b.ToJson());

            e.HasIndex(x => new { x.UserId, x.StartTimeUtc }).IsUnique();

            e.HasOne<Athlete>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Athlete>(e =>
        {
            e.OwnsOne(a => a.HeartRateZones, z =>
            {
                z.Property(p => p.Zone1Max).HasColumnName("HrZone1Max").HasDefaultValue(130);
                z.Property(p => p.Zone2Max).HasColumnName("HrZone2Max").HasDefaultValue(150);
            });

            e.OwnsOne(a => a.PowerZones, z =>
            {
                z.Property(p => p.Zone1Max).HasColumnName("PowerZone1Max").HasDefaultValue(150);
                z.Property(p => p.Zone2Max).HasColumnName("PowerZone2Max").HasDefaultValue(200);
            });

            e.Navigation(a => a.HeartRateZones).IsRequired();
            e.Navigation(a => a.PowerZones).IsRequired();
        });
    }
}
