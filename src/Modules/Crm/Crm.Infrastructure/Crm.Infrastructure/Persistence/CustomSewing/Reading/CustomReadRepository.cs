using Ardalis.Result;
using Ardalis.Specification.EntityFrameworkCore;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Contracts;
using Crm.Application.CustomSewing.GetVersion.Specifications;
using Crm.Application.CustomSewing.GetVersions.Specifications;
using Crm.Application.CustomSewing.Persistence;
using Crm.Domain.CustomSewing.Drafts;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.CustomSewing.Reading;

public sealed class CustomReadRepository(CrmDbContext db) : RepositoryBase<CustomSewingDraft>(db), ICustomReadRepository
{
    public Task<CustomVersionView?> VersionAsync(Guid draftId, Guid versionId, CrmActor actor, CancellationToken ct)
        => Ardalis.Specification.EntityFrameworkCore.SpecificationEvaluator.Default.GetQuery(db.CustomVersions,
            new CustomVersionDetailSpec(draftId, versionId, actor)).FirstOrDefaultAsync(ct);

    public async Task<PagedResult<IReadOnlyList<CustomVersionSummary>>> VersionsAsync(
        Guid draftId, CrmActor actor, int page, int pageSize, CancellationToken ct)
    {
        var count = await Ardalis.Specification.EntityFrameworkCore.SpecificationEvaluator.Default.GetQuery(db.CustomVersions,
            new CustomVersionsCountSpec(draftId, actor)).CountAsync(ct);
        var rows = await Ardalis.Specification.EntityFrameworkCore.SpecificationEvaluator.Default.GetQuery(db.CustomVersions,
            new CustomVersionsPageSpec(draftId, actor, page, pageSize)).ToArrayAsync(ct);
        return new(new PagedInfo(page, pageSize, (long)Math.Ceiling(count / (double)pageSize), count), rows);
    }
}
