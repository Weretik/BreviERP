using Crm.Application.Features.Quotes.Commands.UploadQuotePhoto;

namespace Crm.Api.Endpoints.Quotes.UploadCrmQuotePhoto;

/// <summary>«Змінити фото → Завантажити своє»: stores the photo in the CDN folder of КП photos (R-34).</summary>
internal static class UploadCrmQuotePhotoEndpoint
{
    /// <summary>Hard ceiling for the multipart body; the photo size limit of the library is checked on upload.</summary>
    private const long MaxPhotoRequestBytes = 32L * 1024 * 1024;

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("photos", HandleAsync).WithName("uploadCrmQuotePhoto")
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(MaxPhotoRequestBytes))
            .WithMetadata(new RequestFormLimitsAttribute { MultipartBodyLengthLimit = MaxPhotoRequestBytes });

    private static async Task<IResult> HandleAsync(IFormFile? file, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        if (file is null) return Results.BadRequest(new { error = "Оберіть фото." });
        await using var content = file.OpenReadStream();
        return (await sender.Send(new UploadQuotePhotoCommand(content, file.FileName,
            file.ContentType, file.Length), cancellationToken)).ToHttpResult();
    }
}
