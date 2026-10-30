using System.Linq.Expressions;
using Crm.Application.CustomSewing.Contracts;
using Crm.Domain.CustomSewing.Drafts;

namespace Crm.Application.CustomSewing.Mapping;

public static class CustomDetailProjection
{
    public static readonly Expression<Func<CustomSewingDraft, CustomDetail>> Value = x =>
        new CustomDetail(x.PublicId, x.Revision, x.OwnerId.Value,
            x.Customer == null ? null : x.Customer.PublicId, x.Quote == null ? null : x.Quote.PublicId, x.Configuration,
            x.CurrentVersion == null ? null : new CustomVersionView(x.CurrentVersion.PublicId,
                x.CurrentVersion.Number, x.CurrentVersion.ParentVersion == null ? null : x.CurrentVersion.ParentVersion.PublicId,
                x.CurrentVersion.Configuration, x.CurrentVersion.Basis, x.CurrentVersion.ActorId.Value, x.CurrentVersion.CreatedAt),
            null, x.UpdatedAt, x.Quote == null || !x.Quote.Archived, x.TemplateOrigin);
}
