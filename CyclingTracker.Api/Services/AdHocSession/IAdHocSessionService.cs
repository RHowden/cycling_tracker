using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Shared;

namespace CyclingTracker.Api.Services.AdHocSession;
public interface IAdHocSessionService
{
    Task<Result<Guid>> CreateAsync(CreateAdHocSessionRequest request, Guid userId);
}
