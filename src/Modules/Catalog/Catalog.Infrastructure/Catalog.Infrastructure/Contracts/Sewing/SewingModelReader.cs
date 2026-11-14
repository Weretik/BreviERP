using Catalog.Contracts.Sewing;
using Mediator;
using Catalog.Application.Features.Pricing.Queries.GetSewingArticleSummary;
using Catalog.Application.Features.Product.Queries.ListSewingCompositions;

namespace Catalog.Infrastructure.Contracts.Sewing;

/// <summary>Sewing cards and «Комплектація» of the models through the Catalog sewing queries.</summary>
internal sealed class SewingModelReader(ISender sender) : ISewingModelReader
{
    public async Task<SewingModel?> FindAsync(int productId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSewingArticleSummaryQuery(productId), cancellationToken);
        if (!result.IsSuccess) return null;
        var summary = result.Value;
        return new SewingModel(summary.ProductId, summary.ModelCode,
            summary.Articles.Select(x => new SewingModelArticle(x.Id, x.SortOrder, x.Active, x.FabricId, x.FabricName,
                x.FabricQuantity, x.Costs?.Seamstress)).ToArray(),
            summary.Materials.Select(x => new SewingModelMaterial(x.Kind, x.ReferenceId, x.CostCode, x.Name, x.Quantity,
                x.Unit, x.SortOrder)).ToArray(),
            summary.Operations.Select(x => new SewingModelOperation(x.OperationId, x.Name, x.Quantity, x.ManualMinutes,
                x.SortOrder)).ToArray());
    }

    public async Task<IReadOnlyList<SewingModelComposition>> CompositionsAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListSewingCompositionsQuery(), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Catalog sewing compositions could not be read.");
        return result.Value.Select(model => new SewingModelComposition(model.ProductId, model.ModelCode,
            model.Lines.Select(line => new SewingCompositionLine(line.Kind, line.Article, line.Code, line.Name,
                line.Quantity, line.Unit, line.Comment, line.ArticleId)).ToList())).ToList();
    }
}
