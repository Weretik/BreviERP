using Ardalis.Result;
using BuildingBlocks.Api.Result;
using Microsoft.AspNetCore.Http;

namespace Accounting.Api.Attachments;

internal static class FileResults
{
    /// <summary>The usual mapping, plus 503 with the reason while the file storage is not configured.</summary>
    public static IResult ToFileHttpResult<T>(this Result<T> result) =>
        result.Status == ResultStatus.Unavailable
            ? Results.Json(result.Errors, statusCode: StatusCodes.Status503ServiceUnavailable)
            : result.ToHttpResult();
}
