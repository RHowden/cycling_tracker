using CyclingTracker.Api.Enums;

namespace CyclingTracker.Api.Shared;

public class Result
{
    public bool IsSuccess { get; }

    public ServiceErrorType? ServiceErrorType { get; 
    }
    public string? ErrorMessage { get; }

    protected Result(bool isSuccess, ServiceErrorType? errorType, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ServiceErrorType = errorType;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(ServiceErrorType serviceErrorType, string errorMessage) =>
        new(false, serviceErrorType, errorMessage);
}


public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, ServiceErrorType? serviceErrorType, string? errorMessage)
        : base(isSuccess, serviceErrorType, errorMessage)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, null, null);

    public new static Result<T> Failure(ServiceErrorType serviceErrorType, string errorMessage) =>
        new(false, default, serviceErrorType, errorMessage);
}
