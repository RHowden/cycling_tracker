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

            e.HasIndex(x => new { x.UserId, x.StartTimeUtc }).IsUnique();

            e.HasOne<Athlete>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
