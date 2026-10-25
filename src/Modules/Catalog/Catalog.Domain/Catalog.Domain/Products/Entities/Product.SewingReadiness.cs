using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.Readiness;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

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
                material.Id, material.Kind, material.IsRequired, material.IsReviewed)),
            _operationLines.Select(operation => new OperationReadinessFact(
                operation.Id, operation.IsReviewed)));
    }

    public IReadOnlyList<SewingMaterialLine> GetEffectiveMaterials(ArticleId? articleId = null)
    {
        if (articleId.HasValue && !_articles.Any(a => a.Id == articleId.Value))
            throw new DomainException(ProductErrors.CompositionParentMismatch());
        return _materialLines.Where(x => !x.ArticleId.HasValue || x.ArticleId == articleId)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id.Value).ToArray();
    }

}
