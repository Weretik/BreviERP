using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Access;
using Crm.Domain.CustomSewing;
using Crm.Domain.CustomSewing.Attachments;
using Crm.Domain.CustomSewing.Versions;
using Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Files.Versions;

public sealed class CustomFileVersionWriter(CrmDbContext db)
{
    public async Task PinAsync(CustomSewingVersion version, CrmActor actor, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null) throw new InvalidOperationException("File snapshot requires the version transaction.");
        if (!await db.CustomDrafts.Where(CustomAccess.For(actor)).AnyAsync(x => x.Id == version.DraftId, ct))
            throw new CustomSewingValidationException("private_file_scope_changed");
        var links = await db.Set<CustomDraftAttachment>().Where(x => x.DraftId == version.DraftId)
            .Include(x => x.File).Take(31).ToArrayAsync(ct);
        if (links.Length > 30 || links.Any(x => x.File.State != CustomFileState.Uploaded))
            throw new CustomSewingValidationException("private_file_not_finalized");
        var selected = links.Select(x => x.File.PublicId).ToHashSet();
        if (version.Configuration.Applications.Any(x => x.ArtworkFileId is { } id && !selected.Contains(id)))
            throw new CustomSewingValidationException("ArtworkFileNotVerified");
        db.Set<CustomVersionAttachment>().AddRange(links.Select(link => CustomVersionAttachment.Pin(version, link)));
    }
}
