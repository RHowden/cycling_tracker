using CyclingTracker.Api.Shared;

namespace CyclingTracker.Api.Services;

public interface ISessionDeletionService
{
    Task<Result> DeleteSessionAsync(Guid sessionId, Guid userId);
}
