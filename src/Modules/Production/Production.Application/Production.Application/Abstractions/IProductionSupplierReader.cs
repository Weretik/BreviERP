namespace Production.Application.Abstractions;

/// <summary>«Постачальники» (Reference) for fabric receipts; implemented in the host.</summary>
public interface IProductionSupplierReader
{
    Task<ProductionSupplier?> FindAsync(int id, CancellationToken cancellationToken);
}

public sealed record ProductionSupplier(int Id, string Name);
