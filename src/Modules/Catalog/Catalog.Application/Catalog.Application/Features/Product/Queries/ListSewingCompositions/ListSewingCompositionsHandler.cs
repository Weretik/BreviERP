using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Reference;
using Catalog.Application.Features.Product.Shared.Sewing;

namespace Catalog.Application.Features.Product.Queries.ListSewingCompositions;

public sealed class ListSewingCompositionsHandler(
    ISewingCompositionListReader store,
    IProductReferenceReader referenceNames,
    IProductReferenceCodeReader referenceCodes)
    : IQueryHandler<ListSewingCompositionsQuery, Result<IReadOnlyList<SewingModelCompositionSummary>>>
{
    public async ValueTask<Result<IReadOnlyList<SewingModelCompositionSummary>>> Handle(
        ListSewingCompositionsQuery query, CancellationToken cancellationToken)
    {
        var rows = await store.ReadAllAsync(cancellationToken);
        var names = await referenceNames.GetSnapshotAsync(cancellationToken);
        var codes = await referenceCodes.ReadAsync(cancellationToken);
        var fabricCodes = codes.Fabrics.ToDictionary(x => x.Id, x => x.Code);
        var accessoryCodes = codes.Accessories.ToDictionary(x => x.Id, x => x.Code);

        SewingCompositionSummaryLine ToLine(SewingMaterialView line, string? article, long? articleId)
        {
            var (code, name) = line.ReferenceId is not { } id ? (line.CostCode, (string?)null)
                : line.Kind == "Fabric" ? (fabricCodes.GetValueOrDefault(id), names.Fabrics.GetValueOrDefault(id)?.Name)
                : line.Kind == "Accessory" ? (accessoryCodes.GetValueOrDefault(id), names.GarmentAccessories.GetValueOrDefault(id)?.Name)
                : (line.CostCode, null);
            return new SewingCompositionSummaryLine(line.Kind, article, code ?? line.CostCode, name, line.Quantity,
                line.Unit, line.Comment, articleId);
        }

        var result = rows.Select(row =>
        {
            var skus = row.Articles.ToDictionary(x => x.Id, x => x.SaleSku);
            var variants = row.Articles.SelectMany(article => row.Materials
                .Where(line => line.ArticleId == article.Id)
                .Select(line => ToLine(line, skus.GetValueOrDefault(article.Id), article.Id)));
            var shared = row.Materials.Where(line => line.ArticleId is null).Select(line => ToLine(line, null, null));
            return new SewingModelCompositionSummary(row.ProductId, row.ModelCode, variants.Concat(shared).ToList());
        }).ToList();
        return Result.Success<IReadOnlyList<SewingModelCompositionSummary>>(result);
    }
}
