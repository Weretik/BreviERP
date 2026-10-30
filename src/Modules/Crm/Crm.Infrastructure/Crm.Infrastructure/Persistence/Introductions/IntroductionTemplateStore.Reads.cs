using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Introductions;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes.Introductions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Crm.Infrastructure.Persistence.Introductions;

public sealed partial class IntroductionTemplateStore
{
    public async Task<IntroductionTemplateDetail?> ReadAsync(CrmActor actor, Guid templatePublicId,
        Guid? versionPublicId, CancellationToken cancellationToken)
    {
        if (actor.UserId <= 0 || templatePublicId == Guid.Empty) return null;
        var template = await Accessible(actor).AsNoTracking()
            .SingleOrDefaultAsync(x => x.PublicId == templatePublicId, cancellationToken);
        if (template is null) return null;
        var versions = db.IntroductionTemplateVersions.AsNoTracking().Where(x => x.TemplateId == template.Id);
        var version = versionPublicId.HasValue
            ? await versions.SingleOrDefaultAsync(x => x.PublicId == versionPublicId, cancellationToken)
            : await versions.SingleOrDefaultAsync(x => x.Number == template.VersionNumber, cancellationToken);
        return version is null ? null : ToDetail(template, version);
    }

    public async Task<PagedResult<IReadOnlyList<IntroductionTemplateSummary>>> ListAsync(CrmActor actor,
        bool includeArchived, int page, int pageSize,
        CancellationToken cancellationToken)
    {
        if (page < 1 || pageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(page));
        var query = Accessible(actor).AsNoTracking();
        if (!includeArchived) query = query.Where(x => !x.Archived);
        var count = await query.CountAsync(cancellationToken);
        var templates = await query
            .OrderByDescending(x => x.UpdatedAt).ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(cancellationToken);
        var ids = templates.Select(x => x.Id).ToArray();
        var versions = await db.IntroductionTemplateVersions.AsNoTracking()
            .Where(x => ids.Contains(x.TemplateId) && db.IntroductionTemplates.Any(t =>
                t.Id == x.TemplateId && t.VersionNumber == x.Number))
            .Select(x => new { x.TemplateId, x.PublicId, x.Number, x.Name })
            .ToArrayAsync(cancellationToken);
        var rows = templates.Select(template =>
        {
            var version = versions.Single(x => x.TemplateId == template.Id);
            return new IntroductionTemplateSummary(template.PublicId, template.OwnerId.Value,
                template.Revision, template.Archived, version.PublicId, version.Number,
                version.Name, template.UpdatedAt);
        }).ToArray();
        return new PagedResult<IReadOnlyList<IntroductionTemplateSummary>>(
            new PagedInfo(page, pageSize, (long)Math.Ceiling(count / (double)pageSize), count), rows);
    }

}
