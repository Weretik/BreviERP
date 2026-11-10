using Catalog.Application.Features.Pricing.Sewing.ArticleSummary;
using Catalog.Application.Features.Product.Overview.GetList;
using Catalog.Application.Features.Product.Overview.GetList.DTOs;
using Catalog.Application.Features.Product.Sewing;
using Catalog.Domain.Products.Enums;
using Mediator;
using Production.Application.Abstractions;

namespace Host.Api.Production;

/// <summary>
/// Gives the workshop the Catalog sewing models (about 130) by paging through the product list, and the seamstress
/// labour of each fabric variant from the sewing card, and the material composition of every model.
/// </summary>
internal sealed class ProductionModelReader(ISender sender) : IProductionModelReader
{
    public async Task<IReadOnlyList<ProductionModel>> ListSewingModelsAsync(CancellationToken cancellationToken)
    {
        var rows = new List<ProductOverviewItem>();
        for (var page = 1; ; page++)
        {
            var result = await sender.Send(new GetProductOverviewPageQuery(ProductType.Sewing, null, null, false, false,
                ProductOverviewSort.Code, false, page, GetProductOverviewPageQuery.MaxPageSize), cancellationToken);
            if (!result.IsSuccess) throw new InvalidOperationException("Catalog sewing models could not be read.");
            rows.AddRange(result.Value.Items);
            if (rows.Count >= result.Value.Total || result.Value.Items.Count == 0) break;
        }

        return rows.Select(x => new ProductionModel(x.Code, x.Name, x.IsActive, x.Id, x.Season)).ToArray();
    }

    public async Task<ProductionModelCosts?> ReadCostsAsync(int productId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSewingArticleSummaryQuery(productId), cancellationToken);
        if (!result.IsSuccess) return null;
        var summary = result.Value;
        return new ProductionModelCosts(summary.ProductId, summary.ModelCode,
            summary.Articles.ToDictionary(x => x.Id, x => x.Costs?.Seamstress));
    }

    public async Task<IReadOnlyList<ProductionModelComposition>> ListCompositionsAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListSewingCompositionsQuery(), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Catalog sewing compositions could not be read.");
        return result.Value.Select(model => new ProductionModelComposition(model.ProductId, model.ModelCode,
            model.Lines.Select(line => new ProductionCompositionLine(line.Kind, line.Article, line.Code, line.Name,
                line.Quantity, line.Unit, line.Comment)).ToList())).ToList();
    }
}
