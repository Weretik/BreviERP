using BuildingBlocks.Application.Outbox;
using Crm.Domain.SiteRequests;
using Crm.Application.Features.SiteRequests.Shared;
using Crm.Application.Features.SiteRequests.Shared.Notify;

namespace Crm.Application.Features.SiteRequests.Commands.SubmitSiteRequest;

/// <summary>
/// A request from the «Заявка» form of the public site (LEAD-002 SC-01): saves it with the file on the CDN and, in the
/// same transaction, the outbox message that posts it to the bot's groups (PLT-003 R-07). The post is tried at once;
/// a Telegram failure keeps the request with the reason and the outbox tries again.
/// </summary>
public sealed record SubmitSiteRequestCommand(
    string? Phone,
    string? Workwear,
    string? Quantity,
    string? Comment,
    string? Locale,
    SiteRequestFile? File) : ICommand<Result<SiteRequestReceivedResponse>>;
