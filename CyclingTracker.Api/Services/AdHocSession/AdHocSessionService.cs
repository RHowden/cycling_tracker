using CyclingTracker.Api.Data;
using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Enums;
using CyclingTracker.Api.Models;
using CyclingTracker.Api.Shared;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CyclingTracker.Api.Services.AdHocSession;


public class AdHocSessionService(AppDbContext db, TimeProvider clock) : IAdHocSessionService
{
    public async Task<Result<Guid>> CreateAsync(CreateAdHocSessionRequest request, Guid userId)
    {
        var start = request.StartTime.UtcDateTime;
        var duration = TimeSpan.FromSeconds(request.DurationSeconds);
        var end = start + duration;

        if (start > clock.GetUtcNow().UtcDateTime)
        {
            return Result<Guid>.Failure(ServiceErrorType.ValidationError,
                "Start time can't be in the future.");
        }

        // Intervals overlap when each starts before the other ends. Strict comparisons,
        // so back-to-back sessions (one ends exactly as the next starts) are allowed.
        var overlaps = await db.Set<Session>().AnyAsync(s =>
            s.UserId == userId
            && s.StartTimeUtc < end
            && s.StartTimeUtc + s.Duration > start);

        if (overlaps)
        {
            return Result<Guid>.Failure(ServiceErrorType.Conflict,
                "This overlaps an existing session.");
        }

        // Assumes Session has settable properties. If ActivityMapper (or the DB) already
        // sets Id / CreatedAtUtc, drop those two lines.
        var session = new Session
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StartTimeUtc = start,
            Sport = request.Sport,
            Duration = duration,
            AvgHeartRate = request.AvgHeartRate,
            AvgPowerWatts = request.AvgPowerWatts,
            DistanceMetres = request.DistanceMetres ?? 0,
            CreatedAtUtc = clock.GetUtcNow().UtcDateTime,
        };

        db.Add(session);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg)
        {
            // The overlap check above has a check-then-insert gap, so the unique index
            // on (UserId, StartTimeUtc) is the backstop for two simultaneous requests.
            if (pg.SqlState == PostgresErrorCodes.UniqueViolation
                && pg.ConstraintName == "IX_Sessions_UserId_StartTimeUtc")
            {
                return Result<Guid>.Failure(ServiceErrorType.Conflict,
                    "This overlaps an existing session.");
            }

            if (pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                return Result<Guid>.Failure(ServiceErrorType.ValidationError, "Unknown user.");
            }

            throw;
        }

        return Result<Guid>.Success(session.Id);
    }
}