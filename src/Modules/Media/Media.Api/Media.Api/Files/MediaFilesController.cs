using BuildingBlocks.Api.Authorization;
using BuildingBlocks.Api.Result;
using Mediator;
using Media.Api.Access;
using Media.Application.Contracts;
using Media.Application.Files.DeleteFile;
using Media.Application.Files.GetFile;
using Media.Application.Files.MoveFiles;
using Media.Application.Files.SearchFiles;
using Media.Application.Files.UpdateFile;
using Media.Application.Files.UploadFile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Media.Api.Files;

[ApiController]
[Authorize(Policy = MediaAccess.Policy)]
[Route("api/media/files")]
public sealed class MediaFilesController(ISender sender) : ControllerBase
{
    /// <summary>Photos by name and/or folder, newest first, one page at a time.</summary>
    [HttpGet(Name = "searchMediaFiles")]
    public async Task<ActionResult<MediaFilePage>> Search(
        [FromQuery] int? folderId,
        [FromQuery] string? search,
        [FromQuery] bool includeSubfolders = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 48,
        CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(
            new SearchMediaFilesQuery(folderId, includeSubfolders, search, page, pageSize), cancellationToken));

    [HttpGet("{id:int}", Name = "getMediaFile")]
    public async Task<ActionResult<MediaFileDetailResponse>> Get(int id, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetMediaFileQuery(id), cancellationToken));

    [HttpPost(Name = "uploadMediaFile")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MediaAccess.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MediaAccess.MaxRequestBytes)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MediaFileResponse>> Upload([FromForm] UploadMediaFileRequest request, CancellationToken cancellationToken)
    {
        if (request.File is null) return BadRequest(new { error = "File is required." });
        await using var content = request.File.OpenReadStream();
        return this.ToActionResult(await sender.Send(
            new UploadMediaFileCommand(content, request.File.FileName, request.File.ContentType, request.File.Length, request.FolderId),
            cancellationToken));
    }

    /// <summary>Renames the photo, changes its default alt text or folder.</summary>
    [HttpPut("{id:int}", Name = "updateMediaFile")]
    public async Task<ActionResult<MediaFileResponse>> Update(int id, MediaFileInput input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new UpdateMediaFileCommand(id, input), cancellationToken));

    /// <summary>Moves several photos into one folder.</summary>
    [HttpPost("move", Name = "moveMediaFiles")]
    [AccessAction(AccessActionAttribute.Edit)]
    public async Task<ActionResult> Move(MoveMediaFilesInput input, CancellationToken cancellationToken) =>
        this.ToNoContentResult(await sender.Send(new MoveMediaFilesCommand(input), cancellationToken));

    /// <summary>Deletes the photo from the CDN; 409 while a product uses it.</summary>
    [HttpDelete("{id:int}", Name = "deleteMediaFile")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken) =>
        this.ToNoContentResult(await sender.Send(new DeleteMediaFileCommand(id), cancellationToken));
}
