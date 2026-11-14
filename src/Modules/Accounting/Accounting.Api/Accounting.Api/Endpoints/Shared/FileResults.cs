using Ardalis.Result;
using Microsoft.AspNetCore.Http;
using BuildingBlocks.Api.Errors;

namespace Accounting.Api.Endpoints.Shared;

internal static class FileResults
{
    /// <summary>The usual mapping, plus 503 with the reason while the file storage is not configured.</summary>
    public static IResult ToFileHttpResult<T>(this Result<T> result) =>
        result.Status == ResultStatus.Unavailable
            ? Results.Json(result.Errors, statusCode: StatusCodes.Status503ServiceUnavailable)
            : result.ToHttpResult();
}
