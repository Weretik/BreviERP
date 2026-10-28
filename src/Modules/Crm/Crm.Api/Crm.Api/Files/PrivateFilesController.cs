using Crm.Api.Access;
using Crm.Api.Files.Requests;
using Crm.Application.Files.GetContent;
using Crm.Application.Files.Upload;
using Crm.Application.Files.Validation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Files;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/crm/files")]
public sealed class PrivateFilesController(ISender sender) : ControllerBase
{
    [HttpPost(Name = "uploadCrmPrivateFile")]
    [RequestSizeLimit(21 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 21 * 1024 * 1024)]
    public async Task<ActionResult> Upload([FromForm] UploadPrivateFileRequest request, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        if (Request.Form.Keys.Any(x => x is not ("draftId" or "requestId"))
            || Request.Form.Files.Count != 1 || Request.Form.Files[0].Name != "file") return BadRequest("invalid_upload_fields");
        if (request.File.Length > PrivateFilePolicy.MaximumBytes) return StatusCode(413, new { code = "file_too_large" });
        await using var content = request.File.OpenReadStream();
        var result = await sender.Send(new UploadPrivateFileCommand(request.DraftId, request.RequestId, content,
            request.File.ContentType, actor), ct);
        return result.IsSuccess ? CreatedAtAction(nameof(ReadContent), new { fileId = result.Value.PublicId }, result.Value)
            : PrivateFileHttpErrors.Map(this, result);
    }

    [HttpGet("{fileId:guid}/content", Name = "getCrmPrivateFileContent")]
    public async Task<ActionResult> ReadContent(Guid fileId, Guid? versionId, Guid? quoteId, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await sender.Send(new GetPrivateFileContentQuery(fileId, versionId, quoteId, actor), ct);
        if (!result.IsSuccess) return PrivateFileHttpErrors.Map(this, result);
        Response.Headers.CacheControl = "private, no-store";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        var extension = result.Value.File.ContentType switch { "application/pdf" => "pdf", "image/png" => "png", _ => "jpg" };
        return File(result.Value.Bytes.ToArray(), result.Value.File.ContentType, $"attachment-{fileId:N}.{extension}");
    }
}
