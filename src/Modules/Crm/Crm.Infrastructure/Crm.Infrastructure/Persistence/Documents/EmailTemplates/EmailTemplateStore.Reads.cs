using Ardalis.Result;
using Crm.Application.Documents.EmailTemplates;
using Crm.Domain.Documents.EmailTemplates;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Documents.EmailTemplates;

public sealed partial class EmailTemplateStore
{
    public Task<bool> ExistsAsync(Guid versionId, CancellationToken ct) =>
        db.Set<EmailTemplateVersion>().AsNoTracking().AnyAsync(x => x.PublicId == versionId, ct);

    public async Task<EmailTemplateDetail?> ReadAsync(Guid versionId, CancellationToken ct) =>
        await db.Set<EmailTemplateVersion>().AsNoTracking().Where(x => x.PublicId == versionId)
            .Select(x => new EmailTemplateDetail(x.PublicId, x.Number, x.Name, x.Subject, x.Body, x.CreatedAt)).SingleOrDefaultAsync(ct);

    public async Task<EmailTemplateSettings> ReadActiveAsync(CancellationToken ct)
    {
        var state = await db.Set<EmailTemplateState>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == new EmailTemplateStateId(1), ct);
        if (state?.ActiveVersionId is not { } id) return new EmailTemplateSettings(state?.Revision ?? 1, null);
        var version = await db.Set<EmailTemplateVersion>().AsNoTracking().SingleAsync(x => x.Id == id, ct);
        return new EmailTemplateSettings(state.Revision, Detail(version));
    }

    public async Task<PagedResult<IReadOnlyList<EmailTemplateSummary>>> ListAsync(int page, int pageSize, CancellationToken ct)
    {
        if (page is < 1 or > 1000000 || pageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(page));
        var query = db.Set<EmailTemplateVersion>().AsNoTracking();
        var count = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.Number).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new EmailTemplateSummary(x.PublicId, x.Number, x.Name, x.Subject, x.CreatedAt)).ToArrayAsync(ct);
        return new PagedResult<IReadOnlyList<EmailTemplateSummary>>(new PagedInfo(page, pageSize,
            (long)Math.Ceiling(count / (double)pageSize), count), rows);
    }
}
