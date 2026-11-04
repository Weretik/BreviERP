namespace Production.Application.Abstractions;

/// <summary>«Постачальники» (Reference) for fabric receipts; implemented in the host.</summary>
public interface IProductionSupplierReader
{
    Task<IReadOnlyList<ProductionSupplier>> ListAsync(CancellationToken cancellationToken);
}

public sealed record ProductionSupplier(int Id, string Name);
