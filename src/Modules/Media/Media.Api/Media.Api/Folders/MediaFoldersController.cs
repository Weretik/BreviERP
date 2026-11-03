using BuildingBlocks.Api.Result;
using Mediator;
using Media.Api.Access;
using Media.Application.Contracts;
using Media.Application.Folders.CreateFolder;
using Media.Application.Folders.DeleteFolder;
using Media.Application.Folders.GetFolders;
using Media.Application.Folders.UpdateFolder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Media.Api.Folders;

[ApiController]
[Authorize(Policy = MediaAccess.Policy)]
[Route("api/media/folders")]
public sealed class MediaFoldersController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getMediaFolders")]
    public async Task<ActionResult<IReadOnlyList<MediaFolderResponse>>> List(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetMediaFoldersQuery(), cancellationToken));

    [HttpPost(Name = "createMediaFolder")]
    public async Task<ActionResult<MediaFolderResponse>> Create(MediaFolderInput input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new CreateMediaFolderCommand(input), cancellationToken));

    /// <summary>Renames the folder and/or moves it under another parent.</summary>
    [HttpPut("{id:int}", Name = "updateMediaFolder")]
    public async Task<ActionResult<MediaFolderResponse>> Update(int id, MediaFolderInput input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new UpdateMediaFolderCommand(id, input), cancellationToken));

    /// <summary>Deletes an empty folder.</summary>
    [HttpDelete("{id:int}", Name = "deleteMediaFolder")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken) =>
        this.ToNoContentResult(await sender.Send(new DeleteMediaFolderCommand(id), cancellationToken));
}
