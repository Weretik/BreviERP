using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.Readiness;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

public partial class Product
{
    public void ReplaceComposition(IEnumerable<SewingMaterialLine> materialLines,
        IEnumerable<SewingOperationLine> operationLines, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(materialLines);
        ArgumentNullException.ThrowIfNull(operationLines);
        if (Type != ProductType.Sewing)
            throw new DomainException(ProductErrors.TypeSpecificDataInvalid());
        var materials = materialLines.ToList();
        var operations = operationLines.ToList();
        if (materials.Any(x => x.ProductId != Id || (x.ArticleId.HasValue && !_articles.Any(a => a.Id == x.ArticleId))))
            throw new DomainException(ProductErrors.CompositionParentMismatch());
        if (operations.Any(x => x.ProductId != Id || (x.ArticleId.HasValue && !_articles.Any(a => a.Id == x.ArticleId))))
            throw new DomainException(ProductErrors.CompositionParentMismatch());
        var commonKeys = materials.Where(x => !x.ArticleId.HasValue).Select(x => (x.Kind, x.ReferenceKey)).ToHashSet();
        if (materials.Where(x => x.ArticleId.HasValue).Any(x => commonKeys.Contains((x.Kind, x.ReferenceKey))))
            throw new DomainException(ProductErrors.CompositionScopeConflict());
        if (materials.GroupBy(x => (x.ArticleId, x.Kind, x.ReferenceKey)).Any(g => g.Count() > 1))
            throw new DomainException(ProductErrors.DuplicateCompositionLine());
        if (materials.Count > 2_000 || operations.Count > 2_000)
            throw new DomainException(ProductErrors.CompositionLineLimitReached());
        _materialLines.Clear();
        _materialLines.AddRange(materials);
        _operationLines.Clear();
        _operationLines.AddRange(operations);
        CompositionVersion++;
        MarkAsUpdated(updatedAt);
    }

    public void ReplaceArticleComposition(ArticleId articleId, IEnumerable<SewingMaterialLine> materialLines,
        DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(materialLines);
        var articleLines = materialLines.ToList();
        if (!_articles.Any(a => a.Id == articleId)
            || articleLines.Any(x => x.ProductId != Id || x.ArticleId != articleId))
            throw new DomainException(ProductErrors.CompositionParentMismatch());
        var candidate = _materialLines.Where(x => x.ArticleId != articleId).Concat(articleLines).ToList();
        ReplaceComposition(candidate, _operationLines, updatedAt);
    }

}
