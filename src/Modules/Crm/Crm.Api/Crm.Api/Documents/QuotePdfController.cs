using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Api.Documents.Requests;
using Crm.Application.Documents.GetQuoteArtifact;
using Crm.Application.Documents.RenderQuotePdf;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Documents;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/crm/quotes/{quoteId:guid}/documents")]
public sealed class QuotePdfController(ISender sender) : ControllerBase
{
    [HttpPost(Name = "renderCrmQuotePdf")]
    public async Task<ActionResult> Render(Guid quoteId, RenderQuotePdfRequest request,
        CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await sender.Send(new RenderQuotePdfCommand(quoteId, request.VersionId, actor),
            cancellationToken);
        if (!result.IsSuccess) return this.ToActionResult(result);
        return result.Value.Status == "succeeded" ? Ok(result.Value) :
            AcceptedAtRoute("getCrmQuotePdfArtifact",
                new { quoteId, artifactId = result.Value.ArtifactId }, result.Value);
    }

    [HttpGet("{artifactId:guid}", Name = "getCrmQuotePdfArtifact")]
    public async Task<ActionResult> Metadata(Guid quoteId, Guid artifactId,
        CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await sender.Send(
            new GetQuoteArtifactQuery(quoteId, artifactId, actor), cancellationToken));
    }

    [HttpGet("{artifactId:guid}/content", Name = "downloadCrmQuotePdfArtifact")]
    public async Task<ActionResult> Download(Guid quoteId, Guid artifactId,
        CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await sender.Send(new GetQuoteArtifactContentQuery(quoteId, artifactId, actor),
            cancellationToken);
        return result.IsSuccess ? File(result.Value.Bytes, "application/pdf", $"KP-{artifactId:N}.pdf")
            : this.ToActionResult(result);
    }
}
