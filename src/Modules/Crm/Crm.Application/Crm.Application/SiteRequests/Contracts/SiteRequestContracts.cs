namespace Crm.Application.SiteRequests.Contracts;

/// <summary>The attached file as the public endpoint received it.</summary>
public sealed record SiteRequestFile(Stream Content, string FileName, string ContentType, long Length);

/// <summary>The answer to the site: the request is saved and a manager will call.</summary>
public sealed record SiteRequestReceivedResponse(long Id);

public sealed record SiteRequestResponse(
    long Id,
    string Phone,
    string? Workwear,
    string? Quantity,
    string? Comment,
    string Locale,
    string? FileName,
    string? FileUrl,
    string Status,
    int? OwnerUserId,
    string? OwnerName,
    DateTimeOffset? TakenAt,
    DateTimeOffset? ClosedAt,
    string? Note,
    DateTimeOffset CreatedAt,
    DateTimeOffset? NotifiedAt,
    string? NotifyError,
    int Version);

public sealed record SiteRequestCountsResponse(int New, int InWork, int Mine, int Closed);

public sealed record SiteRequestPageResponse(
    IReadOnlyList<SiteRequestResponse> Items,
    int Total,
    int Page,
    int PageSize,
    SiteRequestCountsResponse Counts);

/// <param name="Action">`Take`, `Done`, `Spam` or `Reopen`.</param>
/// <param name="Version">The version the manager saw; a stale one gets 409.</param>
public sealed record ChangeSiteRequestInput(string Action, int Version, string? Note);
