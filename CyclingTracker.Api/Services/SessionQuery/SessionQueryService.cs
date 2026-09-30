using CyclingTracker.Api.Data;
using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CyclingTracker.Api.Services;

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
                // Postgres / EF translation for TimeSpan to total seconds
                s.Duration.Duration().TotalSeconds,
                s.DistanceMetres,
                s.AvgHeartRate,
                s.AvgPowerWatts
            ))
            .ToListAsync();
    }
}
