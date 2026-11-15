using Identity.Application.Features.Profile.Commands.UploadOwnAvatar;
using BuildingBlocks.Api.Errors;

namespace Identity.Api.Endpoints.Profile.UploadMyAvatar;

/// <summary>Uploads the own photo (multipart <c>file</c>, up to 2 MB); 503 when the photo storage is unavailable.</summary>
internal static class UploadMyAvatarEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("avatar", HandleAsync).WithName("uploadMyAvatar")
            .WithSummary("Upload own photo")
            .WithDescription("JPEG, PNG or WebP up to 2 MB, at least 128×128 px. The server crops the centre square and stores a 256×256 WebP under a new name, then deletes the previous file.").Produces<AvatarResponse>()
            .Accepts<IFormFile>("multipart/form-data").DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(AvatarUploadResults.RequestLimit),
                new RequestFormLimitsAttribute { MultipartBodyLengthLimit = AvatarUploadResults.RequestLimit });

    private static async Task<IResult> HandleAsync(IFormFile file, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        AvatarUploadResults.ToHttpResult(await sender.Send(
            new UploadOwnAvatarCommand(AvatarUploadResults.ToUpload(file)), cancellationToken));
}
