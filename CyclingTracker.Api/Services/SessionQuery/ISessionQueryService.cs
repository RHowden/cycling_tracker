using CyclingTracker.Api.Dtos;

namespace CyclingTracker.Api.Services.SessionQuery;

public interface ISessionQueryService
{
    Task<IReadOnlyCollection<SessionIndexDto>> GetUserSessionsAsync(Guid userId);
    Task<SessionDto?> GetSessionByIdAsync(Guid sessionId, Guid userId);
}
