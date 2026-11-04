namespace Crm.Application.Quotes.Import;

/// <summary>
/// Catalog and Reference rows by their codes for the transfer (R-25); implemented in the host over the Catalog and
/// Reference queries, Crm never reads their tables.
/// </summary>
public interface IQuoteImportLookup
{
    Task<QuoteImportReferences> LoadAsync(CancellationToken cancellationToken);
}

/// <param name="Sewing">Sewing models by code (`MOD-…`).</param>
/// <param name="PpeByCode">PPE products by site article.</param>
/// <param name="PpeByName">PPE products by name in lower case, for a calculator row whose article changed.</param>
/// <param name="Fabrics">Reference fabrics by code (`FAB-…`).</param>
/// <param name="Accessories">Reference garment accessories by code (`FUR-…`).</param>
/// <param name="Operations">Reference operations by code (`OP-…`).</param>
public sealed record QuoteImportReferences(
    IReadOnlyDictionary<string, QuoteImportModel> Sewing,
    IReadOnlyDictionary<string, int> PpeByCode,
    IReadOnlyDictionary<string, int> PpeByName,
    IReadOnlyDictionary<string, QuoteImportReference> Fabrics,
    IReadOnlyDictionary<string, QuoteImportReference> Accessories,
    IReadOnlyDictionary<string, QuoteImportReference> Operations);

/// <param name="Variants">Fabric variants: Catalog Article and its Reference fabric.</param>
public sealed record QuoteImportModel(int ProductId, IReadOnlyList<QuoteImportVariant> Variants);

public sealed record QuoteImportVariant(long ArticleId, int? FabricId, bool IsActive);

public sealed record QuoteImportReference(int Id, string Name, string Unit);
