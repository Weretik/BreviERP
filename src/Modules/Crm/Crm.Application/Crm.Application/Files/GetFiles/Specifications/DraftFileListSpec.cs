using Ardalis.Specification;
using Crm.Application.Files.Contracts;
using Crm.Domain.CustomSewing.Attachments;
using Crm.Domain.CustomSewing.Identifiers;

namespace Crm.Application.Files.GetFiles.Specifications;

public sealed class DraftFileListSpec : Specification<CustomDraftAttachment, PrivateFileView>
{
    public DraftFileListSpec(CustomDraftId draftId, Guid publicId)
    {
        Query.AsNoTracking().Where(x => x.DraftId == draftId).OrderBy(x => x.File.PublicId).Take(31);
        Query.Select(x => new PrivateFileView(x.File.PublicId, publicId, x.File.ContentType, x.File.Length,
            x.File.Sha256, "Uploaded", x.File.ExpiresAt, x.ShowInQuote, x.Production, true));
    }
}
