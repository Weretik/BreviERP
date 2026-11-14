using Catalog.Contracts.Products;
using Catalog.Contracts.Sewing;
using Production.Application.Abstractions;

namespace Production.Infrastructure.Adapters.Catalog;

/// <summary>
/// Gives the workshop the Catalog sewing models (about 130), the seamstress labour of each fabric variant from the
/// sewing card, and the material composition of every model.
/// </summary>
internal sealed class ProductionModelReader(ICatalogProductReader products, ISewingModelReader models)
    : IProductionModelReader
{
    public async Task<IReadOnlyList<ProductionModel>> ListSewingModelsAsync(CancellationToken cancellationToken) =>
        (await products.ListAsync(CatalogProductKind.Sewing, null, cancellationToken))
        .Select(x => new ProductionModel(x.Code!, x.Name, x.IsActive, x.Id, x.Season)).ToArray();

    public async Task<ProductionModelCosts?> ReadCostsAsync(int productId, CancellationToken cancellationToken)
    {
        var model = await models.FindAsync(productId, cancellationToken);
        return model is null
            ? null
            : new ProductionModelCosts(model.ProductId, model.ModelCode,
                model.Articles.ToDictionary(x => x.Id, x => x.SeamstressCost));
    }

    public async Task<IReadOnlyList<ProductionModelComposition>> ListCompositionsAsync(CancellationToken cancellationToken) =>
        (await models.CompositionsAsync(cancellationToken)).Select(model => new ProductionModelComposition(
            model.ProductId, model.ModelCode,
            model.Lines.Select(line => new ProductionCompositionLine(line.Kind, line.Article, line.Code, line.Name,
                line.Quantity, line.Unit, line.Comment, line.ArticleId)).ToList())).ToList();
}
