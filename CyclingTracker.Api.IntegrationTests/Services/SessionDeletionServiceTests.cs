using Microsoft.EntityFrameworkCore;

using CyclingTracker.Api.Models;
using CyclingTracker.Api.Services;
using CyclingTracker.Api.Enums;
using CyclingTracker.Api.Services.SessionDeletion;

namespace CyclingTracker.Api.IntegrationTest.Services;

[Collection("Postgres")]
public class SessionDeletionServiceTests(PostgresFixture pg) : DbTestBase(pg)
{
    [Fact]
    public async Task DeleteSessionAsync_WithValidSessionIdAndMatchingUserId_ReturnsSuccessResult()
    {
        var idToDelete = Guid.NewGuid();
        var targetUser = await SeedAthleteAsync();

        // Seed target user sessions out of chronological order to test sorting logic
        await SeedSessionAsync(targetUser, sessionId: idToDelete);

        var sut = new SessionDeletionService(pg.CreateDb());

        // Act
        var result = await sut.DeleteSessionAsync(idToDelete, targetUser);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteSessionAsync_WithValidSessionIdAndMatchingUserId_DeletesRecordFromDatabase()
    {
        // Arrange
        var idToDelete = Guid.NewGuid();
        var targetUser = await SeedAthleteAsync();
        await SeedSessionAsync(targetUser, sessionId: idToDelete);

        var sut = new SessionDeletionService(pg.CreateDb());

        // Act
        await sut.DeleteSessionAsync(idToDelete, targetUser);

        // Assert
        await using var db = pg.CreateDb();
        var sessionExists = await db.Set<Session>().AnyAsync<Session>(s => s.Id == idToDelete);
        Assert.False(sessionExists);
    }

    [Fact]
    public async Task DeleteSessionAsync_WhenUserIdDoesNotMatchSessionOwner_ReturnsFailureResultWithUnauthorizedError()
    {
        // Arrange
        var idToDelete = Guid.NewGuid();
        var trueOwner = await SeedAthleteAsync();
        var maliciousUser = await SeedAthleteAsync();
        await SeedSessionAsync(trueOwner, sessionId: idToDelete);

        var sut = new SessionDeletionService(pg.CreateDb());

        // Act
        var result = await sut.DeleteSessionAsync(idToDelete, maliciousUser);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.ResourceNotFound, result.ServiceErrorType);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("not found", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteSessionAsync_WhenUserIdDoesNotMatchSessionOwner_DoesNotDeleteRecordFromDatabase()
    {
        // Arrange
        var idToDelete = Guid.NewGuid();
        var trueOwner = await SeedAthleteAsync();
        var maliciousUser = await SeedAthleteAsync();
        await SeedSessionAsync(trueOwner, sessionId: idToDelete);

        var sut = new SessionDeletionService(pg.CreateDb());

        // Act
        await sut.DeleteSessionAsync(idToDelete, maliciousUser);

        // Assert
        await using var db = pg.CreateDb();
        var sessionExists = await db.Set<Session>().AnyAsync(s => s.Id == idToDelete);
        Assert.True(sessionExists);
    }

    [Fact]
    public async Task DeleteSessionAsync_WhenSessionDoesNotExist_ReturnsFailureResultWithNotFoundError()
    {
        // Arrange
        var missingSessionId = Guid.NewGuid();
        var targetUser = await SeedAthleteAsync();

        var sut = new SessionDeletionService(pg.CreateDb());

        // Act
        var result = await sut.DeleteSessionAsync(missingSessionId, targetUser);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.ResourceNotFound, result.ServiceErrorType);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("not found", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteSessionAsync_WhenSessionIdIsEmpty_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var emptySessionId = Guid.Empty;
        var targetUser = await SeedAthleteAsync();

        var sut = new SessionDeletionService(pg.CreateDb());

        // Act
        var result = await sut.DeleteSessionAsync(emptySessionId, targetUser);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.ValidationError, result.ServiceErrorType);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("Session ID cannot be empty", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DeleteSessionAsync_WhenUserIdIsEmpty_ReturnsFailureResultWithValidationError()
    {
        // Arrange
        var idToDelete = Guid.NewGuid();
        var emptyUserId = Guid.Empty;

        var sut = new SessionDeletionService(pg.CreateDb());

        // Act
        var result = await sut.DeleteSessionAsync(idToDelete, emptyUserId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorType.ValidationError, result.ServiceErrorType);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("User ID cannot be empty", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }


    private async Task<Guid> SeedAthleteAsync()
    {
        await using var db = pg.CreateDb();
        var athlete = new Athlete { Id = Guid.NewGuid() };
        db.Add(athlete);
        await db.SaveChangesAsync();
        return athlete.Id;
    }

    private async Task SeedSessionAsync(Guid userId, DateTime? startTime = null, Guid? sessionId = null)
    {
        await using var db = pg.CreateDb();
        var session = new Session
        {
            Id = sessionId ?? Guid.NewGuid(),
            UserId = userId,
            StartTimeUtc = startTime ?? new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc),
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
