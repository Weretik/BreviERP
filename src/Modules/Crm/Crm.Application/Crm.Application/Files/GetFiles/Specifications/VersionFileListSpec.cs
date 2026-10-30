using Ardalis.Specification;
using Crm.Application.Files.Contracts;
using Crm.Domain.CustomSewing.Attachments;

namespace Crm.Application.Files.GetFiles.Specifications;

public sealed class VersionFileListSpec : Specification<CustomVersionAttachment, PrivateFileView>
{
    public VersionFileListSpec(Guid versionId)
    {
        Query.AsNoTracking().Where(x => x.Version.PublicId == versionId).OrderBy(x => x.File.PublicId).Take(31);
        Query.Select(x => new PrivateFileView(x.File.PublicId, x.Version.Draft.PublicId, x.ContentType, x.Length,
            x.Sha256, "Uploaded", x.File.ExpiresAt, x.ShowInQuote, x.Production, true));
    }
}
