using Crm.Api.SiteRequests;
using Crm.Application.Features.SiteRequests.Commands.SubmitSiteRequest;
using Crm.Application.Features.SiteRequests.Shared;

namespace Crm.Api.Endpoints.SiteRequests;

internal static class SubmitPublicSiteRequestEndpoint
{
    public const string RatePolicy = "SiteRequest";
    private const long MaxBody = 11 * 1024 * 1024;

    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("submitPublicSiteRequest")
            .RequireRateLimiting(RatePolicy)
            .WithMetadata(new RequestSizeLimitAttribute(MaxBody))
            .WithMetadata(new RequestFormLimitsAttribute { MultipartBodyLengthLimit = MaxBody })
            .DisableAntiforgery();

    private static async Task<IResult> HandleAsync([FromForm] PublicSiteRequestForm form, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await using var content = form.Attachment?.OpenReadStream();
        var file = form.Attachment is { } attachment && content is not null
            ? new SiteRequestFile(content, attachment.FileName, attachment.ContentType, attachment.Length)
            : null;
        return (await sender.Send(new SubmitSiteRequestCommand(form.Phone, form.Workwear,
            form.Quantity, form.Comment, form.Locale, file), cancellationToken)).ToHttpResult();
    }
}
