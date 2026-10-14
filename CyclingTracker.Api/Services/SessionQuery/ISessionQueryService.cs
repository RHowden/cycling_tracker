using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Shared;

namespace CyclingTracker.Api.Services.SessionQuery;

public interface ISessionQueryService
{
    Task<Result<IReadOnlyList<SessionIndexDto>>> GetUserSessionsAsync(Guid userId);
    Task<Result<SessionDto>> GetSessionByIdAsync(Guid sessionId, Guid userId);
}
