using CyclingTracker.Api.Enums;
using CyclingTracker.Api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace CyclingTracker.Api.Extensions;

public static class ResultExtensions
{
    // For operations with no return value (e.g. delete): 204 on success.
    public static IActionResult ToActionResult(this Result result, ControllerBase controller)
        => result.IsSuccess
            ? controller.NoContent()
            : ToProblem(result.ErrorMessage, controller, result.ServiceErrorType);

    // For operations that return data: 200 with the value by default,
    // or pass onSuccess for something else (e.g. a 201 for a create).
    public static IActionResult ToActionResult<T>(
        this Result<T> result,
        ControllerBase controller,
        Func<T, IActionResult>? onSuccess = null)
        => result.IsSuccess
            ? onSuccess?.Invoke(result.Value) ?? controller.Ok(result.Value)
            : ToProblem(result.ErrorMessage, controller, result.ServiceErrorType);

    private static IActionResult ToProblem(string? message, ControllerBase controller, ServiceErrorType? type = ServiceErrorType.Failure)
    {
        var status = type switch
        {
            ServiceErrorType.ValidationError => StatusCodes.Status400BadRequest,
            ServiceErrorType.InvalidFile => StatusCodes.Status422UnprocessableEntity,
            ServiceErrorType.ResourceNotFound => StatusCodes.Status404NotFound,
            ServiceErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return controller.Problem(detail: message, statusCode: status);
    }
}