using Media.Application.Features.Files.Commands.UploadMediaFile;
using Media.Application.Features.Files.Shared;
using BuildingBlocks.Api.Errors;

namespace Media.Api.Endpoints.Files.UploadMediaFile;

/// <summary>Uploads one photo (multipart: <c>file</c>, optional <c>folderId</c>); size and preview are made on the way.</summary>
internal static class UploadMediaFileEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("uploadMediaFile").Produces<MediaFileResponse>()
            .Accepts<IFormFile>("multipart/form-data").ProducesProblem(StatusCodes.Status400BadRequest)
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(MediaAccess.MaxRequestBytes),
                new RequestFormLimitsAttribute { MultipartBodyLengthLimit = MediaAccess.MaxRequestBytes });

    /// <param name="file">The photo; required.</param>
    /// <param name="folderId">Target folder; empty for the library root.</param>
    private static async Task<IResult> HandleAsync(IFormFile? file, [FromForm] int? folderId,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        if (file is null) return Results.BadRequest(new { error = "File is required." });
        await using var content = file.OpenReadStream();
        return (await sender.Send(new UploadMediaFileCommand(content, file.FileName, file.ContentType, file.Length,
            folderId), cancellationToken)).ToHttpResult();
    }
}

