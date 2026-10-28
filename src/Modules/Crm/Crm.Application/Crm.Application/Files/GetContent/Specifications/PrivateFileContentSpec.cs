using System.Linq.Expressions;
using Ardalis.Specification;
using Crm.Domain.CustomSewing.Attachments;
using Crm.Application.Files.GetContent.Contracts;

namespace Crm.Application.Files.GetContent.Specifications;

public sealed class PrivateFileContentSpec : Specification<CustomPrivateFile, PrivateFileReadDescriptor>
{
    public PrivateFileContentSpec(Guid fileId, Expression<Func<CustomPrivateFile, bool>> access)
    {
        Query.AsNoTracking().Where(access).Where(x => x.PublicId == fileId && x.State == CustomFileState.Uploaded);
        Query.Select(x => new PrivateFileReadDescriptor(x.PublicId, x.Draft.PublicId, x.ContentType, x.Length, x.Sha256));
    }
}
