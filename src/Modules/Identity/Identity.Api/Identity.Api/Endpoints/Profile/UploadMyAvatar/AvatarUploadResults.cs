using Identity.Application.Features.Avatars.Shared;
using BuildingBlocks.Api.Errors;

namespace Identity.Api.Endpoints.Profile.UploadMyAvatar;

/// <summary>HTTP shape of an avatar upload.</summary>
internal static class AvatarUploadResults
{
    // The 2 MB file plus room for multipart headers; larger bodies are refused before they are read.
    public const long RequestLimit = AvatarRules.MaxBytes + 64 * 1024;

    public static AvatarUpload ToUpload(IFormFile file) => new(file.OpenReadStream(), file.Length);

    public static IResult ToHttpResult(Ardalis.Result.Result<AvatarResult> result) =>
        result.Status switch
        {
            Ardalis.Result.ResultStatus.Ok => Results.Ok(new AvatarResponse(result.Value.AvatarUrl)),
            Ardalis.Result.ResultStatus.Unavailable => Results.Json(result.Errors, statusCode: StatusCodes.Status503ServiceUnavailable),
            _ => result.ToHttpResult()
        };
}
