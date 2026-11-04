namespace Production.Application.Abstractions;

/// <summary>Sewing models of Catalog («Товари») by their code; implemented in the host over Catalog queries.</summary>
public interface IProductionModelReader
{
    Task<IReadOnlyList<ProductionModel>> ListSewingModelsAsync(CancellationToken cancellationToken);
}

/// <param name="Code">Model code, for example MOD-025.</param>
public sealed record ProductionModel(string Code, string Name, bool IsActive);
