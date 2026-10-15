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

        // Expected failures come back as a FitParseResult. Anything the parser
        // throws is a genuine bug and propagates, so it returns a 500.
        var parse = parser.Parse(stream);

        if (!parse.Succeeded)
        {
            return parse.Error switch
            {
                FitParseError.NotAFitFile => Result<Guid>.Failure(
                    ServiceErrorType.InvalidFile, "The file could not be read as a FIT file."),
                FitParseError.NoSession => Result<Guid>.Failure(
                    ServiceErrorType.InvalidFile, "The FIT file contains no session."),
                FitParseError.MultipleSessions => Result<Guid>.Failure(
                    ServiceErrorType.InvalidFile, "Multi-session FIT files aren't supported."),
                _ => throw new InvalidOperationException($"Unhandled parse error {parse.Error}"),
            };
        }

        var session = new ActivityMapper().ToEntity(parse.ActivityData, userId);

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