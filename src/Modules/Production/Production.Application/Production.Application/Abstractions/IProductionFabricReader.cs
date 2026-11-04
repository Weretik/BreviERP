namespace Production.Application.Abstractions;

/// <summary>Fabrics of «Тканини» (Reference) by id; implemented in the host.</summary>
public interface IProductionFabricReader
{
    Task<IReadOnlyList<ProductionFabric>> ListAsync(CancellationToken cancellationToken);
}

public sealed record ProductionFabric(int Id, string? Code, string Name);
