using Microsoft.AspNetCore.Http;

namespace Crm.Api.Endpoints.SiteRequests.SubmitPublicSiteRequest;

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
