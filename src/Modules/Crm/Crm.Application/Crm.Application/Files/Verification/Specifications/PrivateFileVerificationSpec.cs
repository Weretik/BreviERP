using Ardalis.Specification;
using Crm.Domain.CustomSewing.Attachments;

namespace Crm.Application.Files.Verification.Specifications;

public sealed class PrivateFileVerificationSpec : Specification<CustomPrivateFile, PrivateFileVerification>
{
    public PrivateFileVerificationSpec(Guid draftId, IReadOnlyCollection<Guid> ids,
        IQueryable<CustomDraftAttachment> drafts, IQueryable<CustomVersionAttachment> versions)
    {
        Query.AsNoTracking().Where(x => x.Draft.PublicId == draftId && ids.Contains(x.PublicId));
        Query.Select(x => new PrivateFileVerification(x.PublicId, x.ContentType, x.Length, x.Sha256,
            x.State, x.ExpiresAt, drafts.Any(link => link.DraftId == x.DraftId && link.FileId == x.Id),
            drafts.Any(link => link.DraftId == x.DraftId && link.FileId == x.Id)
                || versions.Any(link => link.DraftId == x.DraftId && link.FileId == x.Id)));
    }
}
