using CyclingTracker.Api.Enums;
using CyclingTracker.Api.Models;
using CyclingTracker.Api.Shared;
using Microsoft.EntityFrameworkCore;

namespace CyclingTracker.Api.Services;

public class SessionDeletionService(DbContext db) : ISessionDeletionService
{
    public async Task<Result> DeleteSessionAsync(Guid sessionId, Guid userId)
    {
        if (sessionId == Guid.Empty)
        {
            return Result.Failure(ServiceErrorType.ValidationError, "Session ID cannot be empty.");
        }

        if (userId == Guid.Empty)
        {
            return Result.Failure(ServiceErrorType.ValidationError, "User ID cannot be empty.");
        }

        try
        {
            var session = await db.Set<Session>()
                .SingleOrDefaultAsync(s => s.Id == sessionId);

            if (session is null)
            {
                return Result.Failure(ServiceErrorType.ResourceNotFound, "Session not found.");
            }

            if (session.UserId != userId)
            {
                return Result.Failure(ServiceErrorType.AccessDenied, "Unauthorized: Invalid combination of session and user Id.");
            }

            db.Set<Session>().Remove(session);
            await db.SaveChangesAsync();

            return Result.Success();
        }
        catch (DbUpdateException)
        {
            return Result.Failure(ServiceErrorType.Failure, "A database error occurred while trying to delete the session.");
        }
    }
}
