using CyclingTracker.Api.Dtos;

namespace CyclingTracker.Api.Services;

public interface ISessionQueryService
{
    Task<IReadOnlyCollection<SessionIndexDto>> GetUserSessionsAsync(Guid userId);
}
