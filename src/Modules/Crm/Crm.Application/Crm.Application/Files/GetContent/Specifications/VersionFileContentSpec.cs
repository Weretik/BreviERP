using Ardalis.Specification;
using Crm.Domain.CustomSewing.Attachments;
using Crm.Domain.CustomSewing.Versions;
using Crm.Application.Files.GetContent.Contracts;

namespace Crm.Application.Files.GetContent.Specifications;

public sealed class VersionFileContentSpec : Specification<CustomVersionAttachment, PrivateFileReadDescriptor>
{
    public VersionFileContentSpec(Guid fileId, Guid versionId, IQueryable<CustomSewingVersion> accessible)
    {
        Query.AsNoTracking().Where(x => x.File.PublicId == fileId && x.Version.PublicId == versionId
            && x.File.State == CustomFileState.Uploaded && accessible.Any(version => version.Id == x.VersionId));
        Query.Select(x => new PrivateFileReadDescriptor(x.File.PublicId, x.Version.Draft.PublicId,
            x.ContentType, x.Length, x.Sha256));
    }
}
