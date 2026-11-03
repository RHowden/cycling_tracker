using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Enums;
using CyclingTracker.Api.Mappers;
using CyclingTracker.Api.Models;
using CyclingTracker.Api.Services.AdHocSession;
using FitFileParsing;
using Microsoft.EntityFrameworkCore;

namespace CyclingTracker.Api.IntegrationTests.Services;

public class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

[Collection("Postgres")]
public class AdHocSessionServiceTests(PostgresFixture pg) : DbTestBase(pg)
{
    // Existing session used by the overlap tests: 07:30 -> 08:00
    private static readonly DateTime ExistingStart =
        new(2026, 9, 1, 7, 30, 0, DateTimeKind.Utc);
    private const int ExistingDurationSeconds = 1800;

    private static readonly DateTimeOffset Now =
        new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);

    // ----- success path -----

    [Fact]
    public async Task Create_ValidRequest_PersistsSessionAndReturnsId()
    {
        var userId = await SeedAthleteAsync();
        var request = BuildRequest(ExistingStart, 3600, avgHR: 142, avgPower: 205, distance: 32_500);

        var result = await CreateSut().CreateAsync(request, userId);

        Assert.True(result.IsSuccess, result.ErrorMessage);

        await using var db = pg.CreateDb();
        var saved = await db.Set<Session>().SingleAsync(s => s.UserId == userId);

        Assert.Equal(result.Value, saved.Id);
        Assert.Equal(ExistingStart, saved.StartTimeUtc);
        Assert.Equal(SportType.Cycling, saved.Sport);
        Assert.Equal(TimeSpan.FromSeconds(3600), saved.Duration);
        Assert.Equal(142, saved.AvgHeartRate);
        Assert.Equal(205, saved.AvgPowerWatts);
        Assert.Equal(32_500, saved.DistanceMetres, precision: 3);
        Assert.NotEqual(default, saved.CreatedAtUtc);
        Assert.Null(saved.Samples); // ad hoc sessions have no sample data
    }

    [Fact]
    public async Task Create_OptionalFieldsOmitted_StoresNulls()
    {
        var userId = await SeedAthleteAsync();
        var request = BuildRequest(ExistingStart, 1800, avgHR: null, avgPower: null, distance: null);

        var result = await CreateSut().CreateAsync(request, userId);
        Assert.True(result.IsSuccess, result.ErrorMessage);

        await using var db = pg.CreateDb();
        var saved = await db.Set<Session>().SingleAsync(s => s.UserId == userId);

        Assert.Null(saved.AvgHeartRate);
        Assert.Null(saved.AvgPowerWatts);
        Assert.Equal(0, saved.DistanceMetres);
    }

    [Fact]
    public async Task Create_StartTimeWithOffset_IsStoredAsUtc()
    {
        var userId = await SeedAthleteAsync();
        var local = new DateTimeOffset(2026, 7, 1, 9, 0, 0, TimeSpan.FromHours(1)); // 08:00 UTC
        var request = BuildRequest(local, 1800);

        var result = await CreateSut().CreateAsync(request, userId);
        Assert.True(result.IsSuccess, result.ErrorMessage);

        await using var db = pg.CreateDb();
        var saved = await db.Set<Session>().SingleAsync(s => s.UserId == userId);

        Assert.Equal(new DateTime(2026, 7, 1, 8, 0, 0, DateTimeKind.Utc), saved.StartTimeUtc);
    }

    // ----- validation -----

    [Fact]
    public async Task Create_StartInFuture_ReturnsValidationErrorAndWritesNothing()
    {
        var userId = await SeedAthleteAsync();
        var request = BuildRequest(Now.AddMinutes(1), 1800);

        var result = await CreateSut().CreateAsync(request, userId);

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.ValidationError, result.ServiceErrorType);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        Assert.Equal(0, await CountSessionsAsync(userId));
    }

    [Fact]
    public async Task Create_StartExactlyNow_Succeeds()
    {
        var userId = await SeedAthleteAsync();

        var result = await CreateSut().CreateAsync(BuildRequest(Now, 1800), userId);

        Assert.True(result.IsSuccess, result.ErrorMessage);
    }

    [Fact]
    public async Task Create_UnknownUser_ReturnsValidationErrorAndWritesNothing()
    {
        var unknownUserId = Guid.NewGuid(); // no Athlete row

        var result = await CreateSut().CreateAsync(BuildRequest(ExistingStart, 1800), unknownUserId);

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.ValidationError, result.ServiceErrorType);
        Assert.Equal(0, await CountSessionsAsync(unknownUserId));
    }

    // ----- overlap: existing session is 07:30 -> 08:00 -----

    [Theory]
    [InlineData(0, 1800, true)]    // identical interval
    [InlineData(10, 1800, true)]   // starts inside, ends after
    [InlineData(-10, 1800, true)]  // starts before, ends inside
    [InlineData(-10, 3600, true)]  // fully contains existing
    [InlineData(5, 600, true)]     // fully inside existing
    [InlineData(30, 1800, false)]  // starts exactly when existing ends
    [InlineData(-30, 1800, false)] // ends exactly when existing starts
    [InlineData(90, 1800, false)]  // well clear, after
    [InlineData(-90, 1800, false)] // well clear, before
    public async Task Create_AgainstExistingSession_OverlapsOnlyWhenIntervalsIntersect(
        int startOffsetMinutes, int durationSeconds, bool expectConflict)
    {
        var userId = await SeedAthleteAsync();
        await SeedSessionAsync(userId, ExistingStart, ExistingDurationSeconds);

        var request = BuildRequest(ExistingStart.AddMinutes(startOffsetMinutes), durationSeconds);
        var result = await CreateSut().CreateAsync(request, userId);

        if (expectConflict)
        {
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceErrorType.Conflict, result.ServiceErrorType);
            Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
            Assert.Equal(1, await CountSessionsAsync(userId));
        }
        else
        {
            Assert.True(result.IsSuccess, result.ErrorMessage);
            Assert.Equal(2, await CountSessionsAsync(userId));
        }
    }

    [Fact]
    public async Task Create_OverlapsAnotherUsersSession_Succeeds()
    {
        var userA = await SeedAthleteAsync();
        var userB = await SeedAthleteAsync();
        await SeedSessionAsync(userA, ExistingStart, ExistingDurationSeconds);

        var result = await CreateSut().CreateAsync(BuildRequest(ExistingStart, 1800), userB);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, await CountSessionsAsync(userB));
    }

    [Fact]
    public async Task Create_OverlapsOneOfSeveralSessions_ReturnsConflict()
    {
        var userId = await SeedAthleteAsync();
        await SeedSessionAsync(userId, ExistingStart.AddDays(-1), 1800);
        await SeedSessionAsync(userId, ExistingStart, ExistingDurationSeconds);
        await SeedSessionAsync(userId, ExistingStart.AddDays(1), 1800);

        var result = await CreateSut().CreateAsync(BuildRequest(ExistingStart.AddMinutes(15), 600), userId);

        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.Conflict, result.ServiceErrorType);
        Assert.Equal(3, await CountSessionsAsync(userId));
    }

    // ----- race: both requests pass the overlap check, the unique index stops one -----

    [Fact]
    public async Task Create_ConcurrentIdenticalRequests_ExactlyOneSucceedsAndNoneThrow()
    {
        var userId = await SeedAthleteAsync();

        // Each call gets its own service AND its own DbContext
        var tasks = Enumerable.Range(0, 2)
            .Select(_ => CreateSut().CreateAsync(BuildRequest(ExistingStart, 1800), userId))
            .ToArray();

        var results = await Task.WhenAll(tasks); // fails the test if any call throws

        Assert.Equal(1, results.Count(r => r.IsSuccess));
        Assert.All(results.Where(r => !r.IsSuccess),
            r => Assert.Equal(ServiceErrorType.Conflict, r.ServiceErrorType));
        Assert.Equal(1, await CountSessionsAsync(userId));
    }

    // ----- helpers -----

    private AdHocSessionService CreateSut() => new(pg.CreateDb(), new FixedTimeProvider(Now));

    private static CreateAdHocSessionRequest BuildRequest(DateTimeOffset start, int durationSeconds,
        int? avgHR = 150, int? avgPower = 220, double? distance = 20_000) =>
        new(SportType.Cycling, start, durationSeconds, avgHR, avgPower, distance);

    private async Task<int> CountSessionsAsync(Guid userId)
    {
        await using var db = pg.CreateDb();
        return await db.Set<Session>().CountAsync(s => s.UserId == userId);
    }

    private async Task<Guid> SeedAthleteAsync()
    {
        await using var db = pg.CreateDb();
        var athlete = new Athlete { Id = Guid.NewGuid() };
        db.Add(athlete);
        await db.SaveChangesAsync();
        return athlete.Id;
    }

    // Seeds through the real mapper, same as an imported session
    private async Task SeedSessionAsync(Guid userId, DateTime start, int durationSeconds)
    {
        var activity = new ActivityData(start, "Cycling", durationSeconds, 155, 250, 10000f, null);
        await using var db = pg.CreateDb();
        db.Add(new ActivityMapper().ToEntity(activity, userId));
        await db.SaveChangesAsync();
    }
}