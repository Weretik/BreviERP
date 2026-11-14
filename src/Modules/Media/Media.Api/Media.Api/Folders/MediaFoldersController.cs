using BuildingBlocks.Api.Result;
using Mediator;
using Media.Api.Access;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Media.Application.Features.Folders.Commands.CreateMediaFolder;
using Media.Application.Features.Folders.Commands.DeleteMediaFolder;
using Media.Application.Features.Folders.Commands.UpdateMediaFolder;
using Media.Application.Features.Folders.Queries.GetMediaFolders;
using Media.Application.Features.Folders.Shared;

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
