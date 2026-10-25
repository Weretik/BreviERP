using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.Readiness;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Domain.Products.Entities;

namespace UnitTests;

public sealed class SewingArticleTests
{
    [Test]
    public void New_sewing_product_has_an_empty_article_collection()
    {
        var product = Product.Create(ProductId.Create(101), "Model", "Model RU",
            ProductSlug.Create("model"), ProductType.Sewing, DateTimeOffset.UtcNow);

        Assert.That(product.Articles, Is.Empty,
            "Sewing Products need a dedicated Article collection independent of legacy fabric links.");
    }

    [Test]
    public void Product_adds_article_and_rejects_duplicate_sku()
    {
        var product = CreateProduct();
        product.AddArticle(SewingArticle.Create(ArticleId.Create(0), product.Id, " 00246 "), DateTimeOffset.UtcNow);
        Assert.That(product.Articles.Single().SaleSku, Is.EqualTo("00246"));
        Assert.Throws<BuildingBlocks.Domain.Exceptions.DomainException>(() =>
            product.AddArticle(SewingArticle.Create(ArticleId.Create(0), product.Id, "00246"), DateTimeOffset.UtcNow));
        Assert.That(product.Articles, Has.Count.EqualTo(1));
    }

    [Test]
    public void Composition_replacement_is_atomic_when_scope_conflicts()
    {
        var product = CreateProduct();
        var article = SewingArticle.Create(ArticleId.Create(1), product.Id, "00246");
        product.AddArticle(article, DateTimeOffset.UtcNow);
        var shared = SewingMaterialLine.Create(CompositionLineId.Create(1), product.Id, null, SewingMaterialKind.Fabric,
            FabricReferenceId.Create(10), null, null, 3.6m, SewingMaterialUnit.Metre, true, 0);
        product.ReplaceComposition([shared], [], DateTimeOffset.UtcNow);
        var specific = SewingMaterialLine.Create(CompositionLineId.Create(2), product.Id, article.Id, SewingMaterialKind.Fabric,
            FabricReferenceId.Create(10), null, null, 2m, SewingMaterialUnit.Metre, true, 0);
        Assert.Throws<BuildingBlocks.Domain.Exceptions.DomainException>(() => product.ReplaceComposition([shared, specific], [], DateTimeOffset.UtcNow));
        Assert.That(product.MaterialLines, Has.Count.EqualTo(1));
        Assert.That(product.MaterialLines.Single().Id, Is.EqualTo(shared.Id));
    }

    [Test]
    public void Required_operation_manual_minutes_must_be_positive_and_review_metadata_complete()
    {
        var productId = ProductId.Create(101);
        Assert.Throws<BuildingBlocks.Domain.Exceptions.DomainException>(() => SewingOperationLine.Create(
            CompositionLineId.Create(0), productId, null, OperationReferenceId.Create(5), 1m, 0m, 0, true));
        Assert.Throws<BuildingBlocks.Domain.Exceptions.DomainException>(() => SewingOperationLine.Create(
            CompositionLineId.Create(0), productId, null, OperationReferenceId.Create(5), 1m, 4m, 0, true, true));
    }

    [Test]
    public void Effective_materials_include_model_lines_and_selected_article_lines_in_order()
    {
        var product = CreateProduct();
        var article = SewingArticle.Create(ArticleId.Create(4), product.Id, "00246");
        product.AddArticle(article, DateTimeOffset.UtcNow);
        var shared = SewingMaterialLine.Create(CompositionLineId.Create(1), product.Id, null, SewingMaterialKind.Fabric,
            FabricReferenceId.Create(11), null, null, 3.6m, SewingMaterialUnit.Metre, true, 1);
        var articleLine = SewingMaterialLine.Create(CompositionLineId.Create(2), product.Id, article.Id, SewingMaterialKind.Accessory,
            null, AccessoryReferenceId.Create(12), null, 2m, SewingMaterialUnit.Piece, true, 0);
        product.ReplaceComposition([shared, articleLine], [], DateTimeOffset.UtcNow);

        Assert.That(product.GetEffectiveMaterials(article.Id).Select(x => x.Id), Is.EqualTo(new[] { articleLine.Id, shared.Id }));
        Assert.That(product.GetEffectiveMaterials().Select(x => x.Id), Is.EqualTo(new[] { shared.Id }));
    }

    [Test]
    public void Readiness_reports_review_and_missing_composition_blockers()
    {
        var product = CreateProduct();
        var article = SewingArticle.Create(ArticleId.Create(9), product.Id, "SKU-9");
        product.AddArticle(article, DateTimeOffset.UtcNow);
        var operation = SewingOperationLine.Create(CompositionLineId.Create(3), product.Id, null, OperationReferenceId.Create(3),
            1m, null, 0, true);
        product.ReplaceComposition([], [operation], DateTimeOffset.UtcNow);

        var blockers = product.GetCalculationReadinessBlockers(article.Id);
        Assert.That(blockers, Has.Some.Contains("has not been reviewed"));
        Assert.That(blockers, Has.Some.Contains("No effective fabric"));

        var projectedBlockers = SewingCompositionReadiness.Evaluate(article.IsActive,
            product.GetEffectiveMaterials(article.Id).Select(material =>
                new MaterialReadinessFact(material.Id, material.Kind,
                    material.IsRequired, material.IsReviewed)),
            product.OperationLines.Select(line =>
                new OperationReadinessFact(line.Id, line.IsReviewed)));
        Assert.That(projectedBlockers, Is.EqualTo(blockers));
    }

    [Test]
    public void Import_preserves_metre_accessories_and_repeated_operation_times()
    {
        var product = CreateProduct();
        var material = SewingMaterialLine.Create(CompositionLineId.Create(1), product.Id, null,
            SewingMaterialKind.Accessory, null, AccessoryReferenceId.Create(2), null,
            1.9m, SewingMaterialUnit.Metre, true, 0);
        var first = SewingOperationLine.Create(CompositionLineId.Create(2), product.Id, null,
            OperationReferenceId.Create(3), 1m, 20m, 0, true);
        var second = SewingOperationLine.Create(CompositionLineId.Create(3), product.Id, null,
            OperationReferenceId.Create(3), 1m, 86m, 1, true);

        product.ReplaceComposition([material], [first, second], DateTimeOffset.UtcNow);

        Assert.That(product.MaterialLines.Single().Unit, Is.EqualTo(SewingMaterialUnit.Metre));
        Assert.That(product.OperationLines.Select(x => x.ManualMinutes), Is.EqualTo(new decimal?[] { 20m, 86m }));
    }

    private static Product CreateProduct() => Product.Create(ProductId.Create(101), "Model", "Model RU",
        ProductSlug.Create("model"), ProductType.Sewing, DateTimeOffset.UtcNow);
}
