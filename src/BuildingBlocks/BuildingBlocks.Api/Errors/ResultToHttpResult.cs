using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using IResult = Microsoft.AspNetCore.Http.IResult;


namespace BuildingBlocks.Api.Errors;

/// <summary>
/// The one Result-to-HTTP mapping of Minimal API endpoints: success bodies as the MVC controllers answered, every
/// failure a <see cref="ApiProblems"/> ProblemDetails.
/// </summary>
public static class ResultToHttpResult
{
    /// <remarks>A success without a value (a plain <c>Result</c> or a null value) is 204, as MVC's
    /// <c>Ok(null)</c> answered through its no-content formatter.</remarks>
    public static IResult ToHttpResult<T>(this Result<T> result) => result.Status switch
    {
        ResultStatus.Ok => result.Value is null ? Results.NoContent() : Results.Ok(result.Value),
        ResultStatus.NotFound => ApiProblems.NotFound(),
        ResultStatus.Invalid => ApiProblems.Invalid(result.ValidationErrors),
        ResultStatus.Conflict => ApiProblems.Conflict(result.Errors),
        ResultStatus.Forbidden => Results.Forbid(),
        ResultStatus.Unauthorized => Results.Unauthorized(),
        ResultStatus.Unavailable => ApiProblems.Unavailable(result.Errors),
        _ => ApiProblems.Error()
    };

    /// <summary>200 with the mapped value on success, otherwise the shared status mapping.</summary>
    public static IResult ToHttpResult<T, TResponse>(this Result<T> result, Func<T, TResponse> map) =>
        result.Status == ResultStatus.Ok ? Results.Ok(map(result.Value)) : result.ToHttpResult();

    /// <summary>201 with the mapped value and no Location, as the controllers answered; otherwise the shared mapping.</summary>
    public static IResult ToCreatedHttpResult<T, TResponse>(this Result<T> result, Func<T, TResponse> map) =>
        result.Status == ResultStatus.Ok
            ? Results.Json(map(result.Value), statusCode: StatusCodes.Status201Created)
            : result.ToHttpResult();

    /// <summary>204 on success, otherwise the shared status mapping.</summary>
    public static IResult ToNoContentHttpResult(this Ardalis.Result.Result result) =>
        result.IsSuccess ? Results.NoContent() : result.ToHttpResult();

    /// <summary>204 on any success (also a <c>Result&lt;T&gt;</c> whose value the client does not need), otherwise
    /// the shared mapping.</summary>
    public static IResult ToNoContentResult<T>(this Result<T> result) =>
        result.IsSuccess ? Results.NoContent() : result.ToHttpResult();

    /// <summary>As <see cref="ToHttpResult{T}(Result{T})"/>, but an <c>Error</c> is 422: an outside service refused
    /// the work (a sync or a connection test), not a fault of this API.</summary>
    public static IResult ToHttpResultWithRefusal<T>(this Result<T> result) =>
        result.Status == ResultStatus.Error ? ApiProblems.Refused(result.Errors) : result.ToHttpResult();
}
