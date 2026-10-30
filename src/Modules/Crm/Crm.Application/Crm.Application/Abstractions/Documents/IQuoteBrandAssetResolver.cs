namespace Crm.Application.Abstractions.Documents;

// Resolves only server-provisioned, bounded image bytes; never fetches arbitrary URLs.
public interface IQuoteBrandAssetResolver
{
    Task<PublicDocumentImage?> ResolveAsync(string assetKey, CancellationToken cancellationToken);
}
