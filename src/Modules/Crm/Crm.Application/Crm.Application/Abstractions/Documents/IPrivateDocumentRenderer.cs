namespace Crm.Application.Abstractions.Documents;

// Prerequisite port, not the issue/ready operation. Caller pins version and authorizes assets first.
public interface IPrivateDocumentRenderer
{
    Task<byte[]> RenderAsync(PublicDocumentProjection document, CancellationToken cancellationToken);
}
