namespace Catalog.Application.Abstractions.Pricing.Calculations;

public interface ISewingCalculationStore
{
    Task<SavedSewingCalculation?> FindRequestAsync(Guid requestId, CancellationToken cancellationToken);
    Task<SavedSewingCalculation?> FindAsync(int productId, long articleId, Guid id, CancellationToken cancellationToken);
    Task SaveAsync(SavedSewingCalculation calculation, CancellationToken cancellationToken);
}
