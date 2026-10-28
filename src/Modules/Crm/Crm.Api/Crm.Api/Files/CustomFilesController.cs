using Crm.Api.Access;
using Crm.Api.Files.Requests;
using Crm.Application.Files.Contracts;
using Crm.Application.Files.Finalize;
using Crm.Application.Files.GetFiles;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Files;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/crm/custom-sewing/{draftId:guid}/files")]
public sealed class CustomFilesController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getCrmCustomSewingFiles")]
    public async Task<ActionResult> List(Guid draftId, Guid? versionId, Guid? quoteId, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await sender.Send(new GetCustomFilesQuery(draftId, versionId, quoteId, actor), ct);
        return result.IsSuccess ? Ok(result.Value) : PrivateFileHttpErrors.Map(this, result);
    }

    [HttpPut(Name = "setCrmCustomSewingFiles")]
    public async Task<ActionResult> Set(Guid draftId, SetCustomFilesRequest request, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        if (request.Files is null || request.Files.Any(x => x is null)) return BadRequest("invalid_private_file_selection");
        var result = await sender.Send(new SetCustomFilesCommand(draftId, request.RequestId, request.ExpectedRevision,
            request.Files.Select(x => new CustomFileSelection(x.FileId, x.ShowInQuote, x.Production)).ToArray(), actor), ct);
        return result.IsSuccess ? Ok(result.Value) : PrivateFileHttpErrors.Map(this, result);
    }
}
