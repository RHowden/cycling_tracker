using CyclingTracker.Api.Data;
using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Enums;
using CyclingTracker.Api.Models;
using CyclingTracker.Api.Shared;
using Microsoft.EntityFrameworkCore;

namespace CyclingTracker.Api.Services.SessionQuery;

public class SessionQueryService(AppDbContext db) : ISessionQueryService
{

    public async Task<Result<IReadOnlyList<SessionIndexDto>>> GetUserSessionsAsync(Guid userId)
    {
        IReadOnlyList<SessionIndexDto> sessions = await db.Set<Session>()
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
                s.AvgPowerWatts))
            .ToListAsync();

        return Result<IReadOnlyList<SessionIndexDto>>.Success(sessions);
    }

    public async Task<Result<SessionDto>> GetSessionByIdAsync(Guid sessionId, Guid userId)
    {
        var session = await db.Set<Session>()
    .AsNoTracking()
    .SingleOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        return session is null
            ? Result<SessionDto>.Failure(ServiceErrorType.ResourceNotFound, "Session not found.")
            : Result<SessionDto>.Success(SessionDto.From(session));
    }
}
