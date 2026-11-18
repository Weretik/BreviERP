using Crm.Domain.SiteRequests;

namespace Crm.Application.Features.SiteRequests.Shared;

public static class SiteRequestMapper
{
    public static SiteRequestResponse ToResponse(SiteRequest x) => new(x.Id.Value, x.Phone, x.Workwear, x.Quantity,
        x.Comment, x.Locale, x.FileName, x.FileUrl, x.Status.ToString(), x.OwnerUserId?.Value, x.OwnerName, x.TakenAt,
        x.ClosedAt, x.Note, x.CreatedAt, x.NotifiedAt, x.NotifyError, x.Version);
}
