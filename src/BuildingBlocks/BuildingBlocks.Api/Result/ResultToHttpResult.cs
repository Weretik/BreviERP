using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace BuildingBlocks.Api.Result;

/// <summary>
/// The shared Result-to-HTTP mapping of Minimal API endpoints; the same status codes and bodies as the MVC
/// controllers they replaced answered.
/// </summary>
public static class ResultToHttpResult
{
    /// <remarks>A success without a value (a plain <c>Result</c> or a null value) is 204, as MVC's
    /// <c>Ok(null)</c> answered through its no-content formatter.</remarks>
    public static IResult ToHttpResult<T>(this Result<T> result) => result.Status switch
    {
        ResultStatus.Ok => result.Value is null ? Results.NoContent() : Results.Ok(result.Value),
        ResultStatus.NotFound => Results.NotFound(),
        ResultStatus.Invalid => Results.BadRequest(result.ValidationErrors),
        ResultStatus.Conflict => Results.Conflict(result.Errors),
        ResultStatus.Forbidden => Results.Forbid(),
        ResultStatus.Unauthorized => Results.Unauthorized(),
        _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
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
}
