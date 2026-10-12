using CyclingTracker.Api.Data;
using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Enums;
using CyclingTracker.Api.Mappers;
using CyclingTracker.Api.Shared;
using FitFileParsing;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CyclingTracker.Api.Services.FileImport;

public class FitImportService(AppDbContext db, IFitFileParser parser) : IFitImportService
{
    public async Task<Result<Guid>> ImportAsync(IFormFile fitFile, Guid userId)
    {
        await using var stream = fitFile.OpenReadStream();

        ActivityData activity;
        
        try
        {
            activity = parser.Parse(stream);
        }
        catch
        {
            return Result<Guid>.Failure(ServiceErrorType.ValidationError,
             "The file could not be read as a FIT file.");
        }

        var session = new ActivityMapper().ToEntity(activity, userId);

        db.Add(session);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg)
        {
            if (pg.SqlState == PostgresErrorCodes.UniqueViolation
                && pg.ConstraintName == "IX_Sessions_UserId_StartTimeUtc")
            {
                return Result<Guid>.Failure(ServiceErrorType.Conflict,
                 "A session with this start time has already been imported.");
            }

            if (pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                return Result<Guid>.Failure(ServiceErrorType.ValidationError, "Unknown user.");
            }

            throw;
        }
        return Result<Guid>.Success(session.Id);
    }
}