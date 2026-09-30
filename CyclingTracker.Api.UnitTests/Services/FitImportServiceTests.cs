using System.ComponentModel.Design;
using System.Diagnostics;
using CyclingTracker.Api.Data;
using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Mappers;
using CyclingTracker.Api.Models;
using CyclingTracker.Api.Shared;
using FitFileParsing;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace CyclingTracker.Api.Tests;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var db = CreateDb();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public AppDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options);
}

// ---------- Fake parser: hands back a ready-made DTO, no real FIT bytes needed ----------

public class FakeFitParser(ActivityData activity) : IFitFileParser
{
    public ActivityData Parse(Stream fitStream) => activity;
}

public class ThrowingFitParser : IFitFileParser
{
    public ActivityData Parse(Stream fitStream) =>
        throw new InvalidDataException("Corrupt FIT file");
}

// ---------- Tests ----------

public class FitImportServiceTests(PostgresFixture pg) : IClassFixture<PostgresFixture>
{
    private static readonly DateTime StartTime =
        new(2026, 9, 1, 7, 30, 0, DateTimeKind.Utc);

    [Fact]
    public async Task FitImport_ValidFile_PersistsSessionAndReturnsSuccess()
    {
        // Arrange: each test gets its own athlete, so tests stay isolated
        var userId = await SeedAthleteAsync();
        var sut = CreateSut(BuildActivity(StartTime));

        // Act
        Result<SessionDto> result = await sut.ImportAsync(FakeFitFile(), userId);

        // Assert: result
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(result.Value);

        // Assert: row actually exists in Postgres (fresh context, not the service's)
        await using var db = pg.CreateDb();
        var saved = await db.Set<Session>().SingleAsync(s => s.UserId == userId);

        Assert.Equal(StartTime, saved.StartTimeUtc);
        Assert.Equal(SportType.Cycling, saved.Sport);
        Assert.Equal(result.Value!.Id, saved.Id);
    }

    [Fact]
    public async Task ImportAsync_DuplicateUserAndStartTime_FailsAndKeepsSingleRow()
    {
        var userId = await SeedAthleteAsync();

        var first = await CreateSut(BuildActivity(StartTime)).ImportAsync(FakeFitFile(), userId);
        var second = await CreateSut(BuildActivity(StartTime)).ImportAsync(FakeFitFile(), userId);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.False(second.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(second.ErrorMessage));

        Assert.Equal(1, await CountSessionsAsync(userId));
    }
  
    [Fact]
    public async Task ImportAsync_SameStartTimeDifferentUsers_BothSucceed()
    {
        var userA = await SeedAthleteAsync();
        var userB = await SeedAthleteAsync();

        var a = await CreateSut(BuildActivity(StartTime)).ImportAsync(FakeFitFile(), userA);
        var b = await CreateSut(BuildActivity(StartTime)).ImportAsync(FakeFitFile(), userB);

        Assert.True(a.IsSuccess, a.ErrorMessage);
        Assert.True(b.IsSuccess, b.ErrorMessage);
        Assert.Equal(1, await CountSessionsAsync(userA));
        Assert.Equal(1, await CountSessionsAsync(userB));
    }

    [Fact]
    public async Task ImportAsync_SameUserDifferentStartTimes_BothSucceed()
    {
        var userId = await SeedAthleteAsync();

        var first = await CreateSut(BuildActivity(StartTime)).ImportAsync(FakeFitFile(), userId);
        var second = await CreateSut(BuildActivity(StartTime.AddDays(1))).ImportAsync(FakeFitFile(), userId);

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(2, await CountSessionsAsync(userId));
    }

    [Fact]
    public async Task ImportAsync_ConcurrentDuplicateImports_ExactlyOneSucceedsAndNoneThrow()
    {
        var userId = await SeedAthleteAsync();

        // Each call gets its own service AND its own DbContext
        var tasks = Enumerable.Range(0, 2)
            .Select(_ => CreateSut(BuildActivity(StartTime)).ImportAsync(FakeFitFile(), userId))
            .ToArray();

        var results = await Task.WhenAll(tasks); // fails the test if any call throws

        Assert.Equal(1, results.Count(r => r.IsSuccess));
        Assert.Equal(1, results.Count(r => !r.IsSuccess));
        Assert.Equal(1, await CountSessionsAsync(userId));
    }

    [Fact]
    public async Task ImportAsync_UnknownUser_FailsAndWritesNothing()
    {
        var unknownUserId = Guid.NewGuid(); // no Athlete row

        var result = await CreateSut(BuildActivity(StartTime))
            .ImportAsync(FakeFitFile(), unknownUserId);

        Assert.False(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        Assert.Equal(0, await CountSessionsAsync(unknownUserId));
    }


    [Fact]
    public async Task ImportAsync_CorruptFile_FailsAndWritesNothing()
    {
        var userId = await SeedAthleteAsync();
        var sut = new FitImportService(pg.CreateDb(), new ThrowingFitParser());

        var result = await sut.ImportAsync(FakeFitFile(), userId);

        Assert.False(result.IsSuccess);
        Assert.Equal(0, await CountSessionsAsync(userId));
    }

       [Fact]
    public async Task ImportAsync_ValidFile_RoundTripsAllValues()
    {
        var userId = await SeedAthleteAsync();
        var activity = BuildActivity(StartTime, avgHR: 148, avgPower: 210);

        var result = await CreateSut(activity).ImportAsync(FakeFitFile(), userId);
        Assert.True(result.IsSuccess, result.ErrorMessage);

        await using var db = pg.CreateDb();
        var saved = await db.Set<Session>().SingleAsync(s => s.UserId == userId);

        Assert.Equal(activity.StartTime, saved.StartTimeUtc);
        Assert.Equal(ActivityMapper.ToSportType(activity.Type), saved.Sport);
        Assert.Equal(TimeSpan.FromSeconds(activity.Duration), saved.Duration);//
        Assert.Equal(activity.Distance, saved.DistanceMetres, precision: 3);
        Assert.Equal(activity.AvgHr, saved.AvgHeartRate);
        Assert.Equal(activity.AvgPower, saved.AvgPowerWatts);
        Assert.NotEqual(default, saved.CreatedAtUtc);

        // Sport is stored as a string, not an int
        var rawSport = await db.Database
            .SqlQuery<string>($"""SELECT "Sport" AS "Value" FROM "Sessions" WHERE "UserId" = {userId}""")
            .SingleAsync();
        Assert.Equal("Cycling", rawSport);
    }

    [Fact]
    public async Task ImportAsync_NoHeartRateOrPower_StoresNulls()
    {
        var userId = await SeedAthleteAsync();
        var activity = BuildActivity(StartTime, avgHR: null, avgPower: null);

        var result = await CreateSut(activity).ImportAsync(FakeFitFile(), userId);
        Assert.True(result.IsSuccess, result.ErrorMessage);

        await using var db = pg.CreateDb();
        var saved = await db.Set<Session>().SingleAsync(s => s.UserId == userId);

        Assert.Null(saved.AvgHeartRate);
        Assert.Null(saved.AvgPowerWatts);
    }

    // ----- helpers -----

    private async Task<int> CountSessionsAsync(Guid userId)
    {
        await using var db = pg.CreateDb();
        return await db.Set<Session>().CountAsync(s => s.UserId == userId);
    }

    private FitImportService CreateSut(ActivityData activity) =>
        new(pg.CreateDb(), new FakeFitParser(activity));

    private async Task<Guid> SeedAthleteAsync()
    {
        await using var db = pg.CreateDb();
        var athlete = new Athlete { Id = Guid.NewGuid() };
        db.Add(athlete);
        await db.SaveChangesAsync();
        return athlete.Id;
    }

    // The bytes are irrelevant because the parser is faked
    private static IFormFile FakeFitFile()
    {
        var stream = new MemoryStream([0x0E, 0x10]);
        return new FormFile(stream, 0, stream.Length, "file", "ride.fit");
    }

    private static ActivityData BuildActivity(DateTime? start = null, string type = "Cycling", 
        int? avgHR = 155, int? avgPower = 250)
    {
        return new ActivityData(start ?? StartTime, type, 1800.0, avgHR, avgPower, 10000f);
    }
}