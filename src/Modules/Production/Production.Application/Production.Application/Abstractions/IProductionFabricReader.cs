namespace Production.Application.Abstractions;

/// <summary>Fabrics of «Тканини» (Reference) by id; implemented in the host.</summary>
public interface IProductionFabricReader
{
    Task<IReadOnlyList<ProductionFabric>> ListAsync(CancellationToken cancellationToken);
}

/// <param name="Price">Price per metre of «Тканини», UAH; values the stock.</param>
public sealed record ProductionFabric(int Id, string? Code, string Name, decimal? Price = null);
