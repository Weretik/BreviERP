using Catalog.Api.Contracts.Media;
using Catalog.Api.Options;
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
        var file = request.File;
        if (file is null)
            return BadRequest(new { error = "File is required." });

        await using var stream = file.OpenReadStream();

        var result = await sender.Send(
            new UploadMediaFileCommand(
                new UploadMediaFileCommandRequest(
                    stream,
                    file.FileName,
                    file.ContentType,
                    options.BaseFolder,
                    file.Length,
                    options.MaxFileSizeBytes)),
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
