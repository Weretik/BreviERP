namespace Production.Application.Abstractions;

/// <summary>Sewing models of Catalog («Товари»); implemented in the host over Catalog queries.</summary>
public interface IProductionModelReader
{
    Task<IReadOnlyList<ProductionModel>> ListSewingModelsAsync(CancellationToken cancellationToken);

    /// <summary>The seamstress labour («розцінка пошиття», UAH per item) of every fabric variant of a model.</summary>
    /// <returns>Null when the product is not a sewing model.</returns>
    Task<ProductionModelCosts?> ReadCostsAsync(int productId, CancellationToken cancellationToken);
}

/// <param name="Code">Model code, for example MOD-025.</param>
public sealed record ProductionModel(string Code, string Name, bool IsActive);

/// <param name="SewingRates">By Catalog article (fabric variant) id; null when the variant is not calculated.</param>
public sealed record ProductionModelCosts(int ProductId, string? ModelCode, IReadOnlyDictionary<long, decimal?> SewingRates);
