using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Crm.Application.Features.SiteRequests.Commands.SubmitSiteRequest;
using Crm.Application.Features.SiteRequests.Shared;

namespace Crm.Api.SiteRequests;

/// <summary>
/// The «Заявка» form of the public site (LEAD-002): anonymous, a phone and optionally a file up to 10 MB, limited
/// by the host's rate policy <see cref="RatePolicy"/>.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/public/site-requests")]
public sealed class PublicSiteRequestsController(ISender sender) : ControllerBase
{
    public const string RatePolicy = "SiteRequest";
    private const long MaxBody = 11 * 1024 * 1024;

    [HttpPost(Name = "submitPublicSiteRequest")]
    [EnableRateLimiting(RatePolicy)]
    [RequestSizeLimit(MaxBody)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxBody)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<SiteRequestReceivedResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SiteRequestReceivedResponse>> Submit([FromForm] PublicSiteRequestForm form,
        CancellationToken cancellationToken)
    {
        await using var content = form.Attachment?.OpenReadStream();
        var file = form.Attachment is { } attachment && content is not null
            ? new SiteRequestFile(content, attachment.FileName, attachment.ContentType, attachment.Length)
            : null;
        return this.ToActionResult(await sender.Send(new SubmitSiteRequestCommand(form.Phone, form.Workwear,
            form.Quantity, form.Comment, form.Locale, file), cancellationToken));
    }
}

/// <summary>The fields of the site form; <see cref="Attachment"/> is the optional brief or layout.</summary>
public sealed class PublicSiteRequestForm
{
    public string? Phone { get; init; }
    public string? Workwear { get; init; }
    public string? Quantity { get; init; }
    public string? Comment { get; init; }

    /// <summary>`uk` or `ru`: the language of the page.</summary>
    public string? Locale { get; init; }

    public IFormFile? Attachment { get; init; }
}
