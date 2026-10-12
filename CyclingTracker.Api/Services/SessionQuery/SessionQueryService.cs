using CyclingTracker.Api.Data;
using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CyclingTracker.Api.Services.SessionQuery;

public class SessionQueryService(AppDbContext db) : ISessionQueryService
{

    public async Task<IReadOnlyCollection<SessionIndexDto>> GetUserSessionsAsync(Guid userId)
    {
        return await db.Set<Session>()
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartTimeUtc)
            .Select(s => new SessionIndexDto(
                s.Id,
                s.StartTimeUtc,
                s.Sport.ToString(),
                s.Duration.Duration().TotalSeconds,
                s.DistanceMetres,
                s.AvgHeartRate,
                s.AvgPowerWatts
            ))
            .ToListAsync();
    }

    public async Task<SessionDto?> GetSessionByIdAsync(Guid sessionId, Guid userId)
    {
        var row = await db.Set<Session>()
            .AsNoTracking()
            .Where(s => s.Id == sessionId && s.UserId == userId)
            .Select(s => new
            {
                s.Id,
                s.StartTimeUtc,
                s.Sport,
                s.Duration,
                s.DistanceMetres,
                s.AvgHeartRate,
                s.AvgPowerWatts,
                s.Samples
            })
            .SingleOrDefaultAsync();

        if (row is null)
        {
            return null;
        }

        return new SessionDto(
            row.Id,
            row.StartTimeUtc,
            row.Sport.ToString(),
            row.Duration.TotalSeconds,
            row.DistanceMetres,
            row.AvgHeartRate,
            row.AvgPowerWatts,
            row.Samples is null
                ? null
                : new SessionSamplesDto(
                    row.Samples.Time,
                    row.Samples.HeartRate,
                    row.Samples.Power,
                    row.Samples.Speed));
    }
}
