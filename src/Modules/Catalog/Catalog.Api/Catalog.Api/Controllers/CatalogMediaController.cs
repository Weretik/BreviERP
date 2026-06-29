using Catalog.Api.Contracts.Media;
using Catalog.Api.Options;
using Catalog.Api.Validation;
using Catalog.Application.Features.Media.Delete;
using Catalog.Application.Features.Media.GetList;
using Catalog.Application.Features.Media.Upload;
using Catalog.Application.Features.Media.Upload.DTOs;

namespace Catalog.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/catalog/media")]
public sealed class CatalogMediaController(
    ISender sender,
    IOptions<CatalogMediaUploadOptions> uploadOptions) : ControllerBase
{
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    [HttpGet]
    [ProducesResponseType(typeof(List<MediaFileListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MediaFileListItemResponse>>> GetList(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMediaFilesQuery(), cancellationToken);
        if (!result.IsSuccess)
            return this.ToActionResult(result);

        return Ok(result.Value
            .Select(x => new MediaFileListItemResponse(
                x.Id,
                x.OriginalFileName,
                x.PublicUrl,
                x.ContentType,
                x.StorageKey,
                x.Status))
            .ToList());
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [DisableRequestSizeLimit]
    [ProducesResponseType(typeof(UploadMediaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadMediaResponse>> Upload(
        [FromForm] UploadMediaRequest request,
        CancellationToken cancellationToken)
    {
        var options = uploadOptions.Value;
        var maxFileSizeBytes = options.MaxFileSizeBytes;
        var file = request.File;
        if (file is null)
            return BadRequest(new { error = "File is required." });

        if (file.Length <= 0)
            return BadRequest(new { error = "File size must be greater than zero." });

        if (file.Length > maxFileSizeBytes)
            return BadRequest(new { error = $"File size must not exceed {maxFileSizeBytes / (1024 * 1024)} MB." });

        if (!AllowedContentTypes.Contains(file.ContentType))
            return BadRequest(new { error = "Only image/jpeg, image/png, image/webp are allowed." });

        await using var stream = file.OpenReadStream();
        if (!await ImageFileSignatureValidator.IsValidAsync(stream, file.ContentType, cancellationToken))
            return BadRequest(new { error = "File content does not match the declared image type." });

        var result = await sender.Send(
            new UploadMediaFileCommand(
                new UploadMediaFileCommandRequest(
                    stream,
                    file.FileName,
                    file.ContentType,
                    options.BaseFolder,
                    file.Length)),
            cancellationToken);

        if (!result.IsSuccess)
            return this.ToActionResult(result);

        return Ok(new UploadMediaResponse(
            result.Value.MediaFileId,
            result.Value.StorageKey,
            result.Value.PublicUrl,
            result.Value.ContentType,
            file.FileName));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteMediaFileCommand(id), cancellationToken);

        if (!result.IsSuccess)
            return this.ToActionResult(result);

        return NoContent();
    }
}
