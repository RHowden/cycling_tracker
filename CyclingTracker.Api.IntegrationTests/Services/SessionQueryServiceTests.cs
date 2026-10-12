using CyclingTracker.Api.Models;
using CyclingTracker.Api.Services.SessionQuery;

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

        // Assert
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

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }


    [Fact]
    public async Task GetSessionById_SessionExistsAndBelongsToUser_ReturnsMappedDto()
    {
        // Arrange
        var userId = await SeedAthleteAsync();
        var sessionId = Guid.NewGuid();
        await SeedSessionAsync(userId, new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc), sessionId);

        var sut = new SessionQueryService(pg.CreateDb());

        // Act
        var result = await sut.GetSessionByIdAsync(sessionId, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(sessionId, result.Id);
    }

    [Fact]
    public async Task GetSessionById_SessionBelongsToDifferentUser_ReturnsNull()
    {
        // Arrange
        var userA = await SeedAthleteAsync();
        var userB = await SeedAthleteAsync();
        var sessionId = Guid.NewGuid();
        await SeedSessionAsync(userA, new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc), sessionId);

        var sut = new SessionQueryService(pg.CreateDb());

        // Act - User B tries to look at User A's session
        var result = await sut.GetSessionByIdAsync(sessionId, userB);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSessionById_SessionWithSamples_ReturnsSampleColumns()
    {
        var userId = await SeedAthleteAsync();
        var sessionId = Guid.NewGuid();
        var samples = new SampleData
        {
            Time = [0, 1, 2, 300],
            HeartRate = [130, 131, null, 140],
            Power = [200, null, 210, 250],
            Speed = [8.5, 8.75, null, 9.0]
        };
        await SeedSessionAsync(userId, new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc), sessionId, samples);

        var sut = new SessionQueryService(pg.CreateDb());

        var result = await sut.GetSessionByIdAsync(sessionId, userId);

        Assert.NotNull(result);
        Assert.NotNull(result.Samples);
        Assert.Equal(samples.Time, result.Samples.Time);
        Assert.Equal(samples.HeartRate, result.Samples.HeartRate);
        Assert.Equal(samples.Power, result.Samples.Power);
        Assert.Equal(samples.Speed, result.Samples.Speed);
    }

    [Fact]
    public async Task GetSessionById_SessionWithoutSamples_ReturnsNullSamples()
    {
        var userId = await SeedAthleteAsync();
        var sessionId = Guid.NewGuid();
        await SeedSessionAsync(userId, new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc), sessionId);

        var sut = new SessionQueryService(pg.CreateDb());

        var result = await sut.GetSessionByIdAsync(sessionId, userId);

        Assert.NotNull(result);
        Assert.Null(result.Samples);
    }

    [Fact]
    public async Task GetSessionById_SamplesBelongToDifferentUser_ReturnsNullAndLeaksNothing()
    {
        var userA = await SeedAthleteAsync();
        var userB = await SeedAthleteAsync();
        var sessionId = Guid.NewGuid();
        var samples = new SampleData { Time = [0], HeartRate = [130], Power = [200], Speed = [8.5] };
        await SeedSessionAsync(userA, new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc), sessionId, samples);

        var sut = new SessionQueryService(pg.CreateDb());

        var result = await sut.GetSessionByIdAsync(sessionId, userB);

        Assert.Null(result);
    }
    private async Task<Guid> SeedAthleteAsync()
    {
        await using var db = pg.CreateDb();
        var athlete = new Athlete { Id = Guid.NewGuid() };
        db.Add(athlete);
        await db.SaveChangesAsync();
        return athlete.Id;
    }

    private async Task SeedSessionAsync(Guid userId, DateTime startTime, Guid? sessionId = null,
    SampleData? samples = null)
{
    await using var db = pg.CreateDb();
    var session = new Session
    {
        Id = sessionId ?? Guid.NewGuid(),
        UserId = userId,
        StartTimeUtc = startTime,
        Sport = SportType.Cycling,
        Duration = TimeSpan.FromHours(1),
        DistanceMetres = 30000.0,
        AvgHeartRate = 150,
        AvgPowerWatts = 230,
        Samples = samples,
        CreatedAtUtc = DateTime.UtcNow
    };
    db.Add(session);
    await db.SaveChangesAsync();
}
}
