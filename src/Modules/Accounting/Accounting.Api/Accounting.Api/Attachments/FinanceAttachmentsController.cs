using Accounting.Api.Access;
using Accounting.Api.Results;
using Accounting.Application.Attachments;
using Accounting.Application.Contracts;
using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Api.Attachments;

/// <summary>
/// Files of an income or expense («Файли» of the operation panel). They live in the private storage and are read only
/// here; 503 while the storage is not configured.
/// </summary>
[ApiController]
[Authorize(Policy = AccountingAccess.Policy)]
public sealed class FinanceAttachmentsController(ISender sender) : ControllerBase
{
    /// <summary>The file and a little room for the form around it.</summary>
    private const long MaxRequestBytes = 11 * 1024 * 1024;

    [HttpGet("api/accounting/transactions/{id:guid}/attachments", Name = "getFinanceAttachments")]
    public async Task<ActionResult<IReadOnlyList<FinanceAttachmentResponse>>> List(Guid id,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetFinanceAttachmentsQuery(id), cancellationToken));

    [HttpPost("api/accounting/transactions/{id:guid}/attachments", Name = "uploadFinanceAttachment")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<FinanceAttachmentResponse>> Upload(Guid id,
        [FromForm] UploadFinanceAttachmentRequest request, CancellationToken cancellationToken)
    {
        if (request.File is null) return BadRequest(new { error = "Оберіть файл." });
        await using var content = request.File.OpenReadStream();
        return this.ToFileActionResult(await sender.Send(
            new UploadFinanceAttachmentCommand(id, content, request.File.FileName, AccountingAccess.By(User)),
            cancellationToken));
    }

    [HttpDelete("api/accounting/attachments/{id:guid}", Name = "deleteFinanceAttachment")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        this.ToNoContentResult(await sender.Send(new DeleteFinanceAttachmentCommand(id, AccountingAccess.By(User)),
            cancellationToken));

    /// <summary>The content, shown in the browser: never cached, never sniffed into another type.</summary>
    [HttpGet("api/accounting/attachments/{id:guid}/content", Name = "getFinanceAttachmentContent")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK, "application/pdf", "image/jpeg", "image/png")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult> Content(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFinanceAttachmentContentQuery(id), cancellationToken);
        if (!result.IsSuccess) return this.ToFileActionResult(result);
        Response.Headers.CacheControl = "private, no-store";
        Response.Headers.XContentTypeOptions = "nosniff";
        Response.Headers.ContentDisposition = new System.Net.Mime.ContentDisposition
        {
            Inline = true,
            FileName = result.Value.FileName,
        }.ToString();
        return File(result.Value.Content, result.Value.ContentType);
    }
}
