using Crm.Domain.SiteRequests;
using Crm.Application.Features.SiteRequests.Shared;

namespace Crm.Application.Features.SiteRequests.Commands.ChangeSiteRequest;

/// <summary>
/// «Беру», «Готово», «Не заявка» or «Повернути в нові» (LEAD-002 SC-04). Anyone with the page takes a free request;
/// only its owner or an Admin closes or frees it.
/// </summary>
public sealed record ChangeSiteRequestCommand(int UserId, bool IsAdmin, long Id, ChangeSiteRequestInput Input)
    : ICommand<Result<SiteRequestResponse>>;
