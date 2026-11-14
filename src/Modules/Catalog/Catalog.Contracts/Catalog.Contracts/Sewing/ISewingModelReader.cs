namespace Catalog.Contracts.Sewing;

/// <summary>Sewing cards of the Catalog models for other modules.</summary>
public interface ISewingModelReader
{
    Task<SewingModel?> FindAsync(int productId, CancellationToken cancellationToken);

    Task<IReadOnlyList<SewingModelComposition>> CompositionsAsync(CancellationToken cancellationToken);
}
