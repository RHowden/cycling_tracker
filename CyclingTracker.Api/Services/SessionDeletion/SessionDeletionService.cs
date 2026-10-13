using System.Data.Common;
using CyclingTracker.Api.Data;
using CyclingTracker.Api.Enums;
using CyclingTracker.Api.Models;
using CyclingTracker.Api.Shared;
using Microsoft.EntityFrameworkCore;

namespace CyclingTracker.Api.Services.SessionDeletion;

public class SessionDeletionService(AppDbContext db) : ISessionDeletionService
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
            var deleted = await db.Set<Session>()
                .Where(s => s.Id == sessionId && s.UserId == userId)
                .ExecuteDeleteAsync();

            return deleted == 0
                ? Result.Failure(ServiceErrorType.ResourceNotFound, "Session not found.")
                : Result.Success();
        }
        catch (DbException)
        {
            return Result.Failure(ServiceErrorType.Failure, "A database error occurred while trying to delete the session.");
        }
    }
}