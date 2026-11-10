namespace Production.Application.Abstractions;

/// <summary>Sewing models of Catalog («Товари»); implemented in the host over Catalog queries.</summary>
public interface IProductionModelReader
{
    Task<IReadOnlyList<ProductionModel>> ListSewingModelsAsync(CancellationToken cancellationToken);

    /// <summary>The seamstress labour («розцінка пошиття», UAH per item) of every fabric variant of a model.</summary>
    /// <returns>Null when the product is not a sewing model.</returns>
    Task<ProductionModelCosts?> ReadCostsAsync(int productId, CancellationToken cancellationToken);

    /// <summary>The material composition of every Catalog sewing model («Комплектація» of the model card).</summary>
    Task<IReadOnlyList<ProductionModelComposition>> ListCompositionsAsync(CancellationToken cancellationToken);
}

/// <param name="Code">Model code, for example MOD-025.</param>
/// <param name="ProductId">The Catalog product, for a link to its card.</param>
/// <param name="Season">«Літо» or «Зима»; null when not set.</param>
public sealed record ProductionModel(string Code, string Name, bool IsActive, int? ProductId = null, string? Season = null);

/// <param name="SewingRates">By Catalog article (fabric variant) id; null when the variant is not calculated.</param>
public sealed record ProductionModelCosts(int ProductId, string? ModelCode, IReadOnlyDictionary<long, decimal?> SewingRates);

/// <summary>A Catalog sewing model with its material lines: fabric variants first, then the shared lines.</summary>
public sealed record ProductionModelComposition(int ProductId, string? ModelCode, IReadOnlyList<ProductionCompositionLine> Lines);

/// <param name="Kind">Fabric, Accessory or OtherCost.</param>
/// <param name="Article">The sale SKU of a fabric variant; null for a line of the whole model.</param>
/// <param name="Unit">Metre, Piece, Service or Kilogram.</param>
public sealed record ProductionCompositionLine(
    string Kind, string? Article, string? Code, string? Name, decimal Quantity, string Unit, string? Comment);
