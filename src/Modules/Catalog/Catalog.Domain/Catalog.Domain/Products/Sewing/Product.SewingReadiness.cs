using Catalog.Domain.Products.Readiness;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.Sewing.Composition;
using Catalog.Domain.Products.Sewing.ValueObjects;

namespace Catalog.Domain.Products;

public partial class Product
{
    public IReadOnlyList<string> GetCalculationReadinessBlockers(ArticleId? articleId = null)
    {
        if (articleId.HasValue && !_articles.Any(a => a.Id == articleId.Value))
            return ["Article does not belong to this product."];
        var materials = _materialLines.Where(x => !x.ArticleId.HasValue || x.ArticleId == articleId);
        var articleIsActive = !articleId.HasValue || _articles.Single(a => a.Id == articleId.Value).IsActive;
        return SewingCompositionReadiness.Evaluate(articleIsActive,
            materials.Select(material => new MaterialReadinessFact(
                material.Id, material.Kind)),
            _operationLines.Select(operation => new OperationReadinessFact(
                operation.Id)));
    }

    public DomainResult<IReadOnlyList<SewingMaterialLine>> GetEffectiveMaterials(ArticleId? articleId = null)
    {
        if (articleId.HasValue && !_articles.Any(a => a.Id == articleId.Value))
            return ProductErrors.CompositionParentMismatch();
        return DomainResult<IReadOnlyList<SewingMaterialLine>>.Success(_materialLines
            .Where(x => !x.ArticleId.HasValue || x.ArticleId == articleId)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id.Value).ToArray());
    }

}
