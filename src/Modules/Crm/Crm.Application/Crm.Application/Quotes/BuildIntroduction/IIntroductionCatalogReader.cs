namespace Crm.Application.Quotes.BuildIntroduction;

/// <summary>Reads current approved Catalog model, fabric metadata and server price for one selection.</summary>
public interface IIntroductionCatalogReader
{
    Task<IntroductionCatalogOffer?> ReadAsync(int productId, long articleId,
        CancellationToken cancellationToken);
}
