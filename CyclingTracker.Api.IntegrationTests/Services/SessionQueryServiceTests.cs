using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Models;
using CyclingTracker.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CyclingTracker.Api.IntegrationTest.Services;

[Collection("Postgres")]
public class SessionQueryServiceTests(PostgresFixture pg) : DbTestBase(pg)
{
    [Fact]
    public async Task GetUserSessions_WhenSessionsExist_ReturnsOnlyBelongingToUser_OrderedByNewestFirst()
    {
        // Arrange: Use separate athletes to prevent state bleed
        var targetUser = await SeedAthleteAsync();
        var otherUser = await SeedAthleteAsync();

        // Seed target user sessions out of chronological order to test sorting logic
        await SeedSessionAsync(targetUser, new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc));
        await SeedSessionAsync(targetUser, new DateTime(2026, 9, 3, 8, 0, 0, DateTimeKind.Utc)); // Newest
        await SeedSessionAsync(targetUser, new DateTime(2026, 9, 2, 8, 0, 0, DateTimeKind.Utc));

        // Seed noise data for a completely separate user to ensure isolation
        await SeedSessionAsync(otherUser, new DateTime(2026, 9, 3, 8, 0, 0, DateTimeKind.Utc));

        var sut = new SessionQueryService(pg.CreateDb());

        // Act
        var result = await sut.GetUserSessionsAsync(targetUser);

        // Assert: 🔴 This throws NotImplementedException right now!
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        
        // Confirm reverse-chronological ordering works flawlessly
        Assert.Collection(result,
            first => Assert.Equal(new DateTime(2026, 9, 3, 8, 0, 0, DateTimeKind.Utc), first.StartTimeUtc),
            second => Assert.Equal(new DateTime(2026, 9, 2, 8, 0, 0, DateTimeKind.Utc), second.StartTimeUtc),
            third => Assert.Equal(new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc), third.StartTimeUtc)
        );
    }

    [Fact]
    public async Task GetUserSessions_WhenUserHasNoSessions_ReturnsEmptyCollection()
    {
        // Arrange
        var emptyUser = await SeedAthleteAsync();
        var sut = new SessionQueryService(pg.CreateDb());

        // Act
        var result = await sut.GetUserSessionsAsync(emptyUser);

        // Assert: 🔴 Also goes red on NotImplementedException
        Assert.NotNull(result);
        Assert.Empty(result);
    }


    private async Task<Guid> SeedAthleteAsync()
    {
        await using var db = pg.CreateDb();
        var athlete = new Athlete { Id = Guid.NewGuid() };
        db.Add(athlete);
        await db.SaveChangesAsync();
        return athlete.Id;
    }

    private async Task SeedSessionAsync(Guid userId, DateTime startTime)
    {
        await using var db = pg.CreateDb();
        var session = new Session
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StartTimeUtc = startTime,
            Sport = SportType.Cycling,
            Duration = TimeSpan.FromHours(1),
            DistanceMetres = 30000.0,
            AvgHeartRate = 150,
            AvgPowerWatts = 230,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Add(session);
        await db.SaveChangesAsync();
    }
}
