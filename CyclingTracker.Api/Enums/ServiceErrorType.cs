namespace CyclingTracker.Api.Enums;

public enum ServiceErrorType
{
    None,
    ValidationError,  // The input parameters were invalid
    ResourceNotFound, // The requested entity does not exist in the system
    AccessDenied,     // The current user context violates a business permission rule
    Conflict,         // e.g., Concurrency issues, duplicate keys
    Failure           // Generic internal infrastructure/database failure
}