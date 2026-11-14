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
            ProductSlug.Create("model").Value, ProductType.Sewing, DateTimeOffset.UtcNow).Value;

        Assert.That(product.Articles, Is.Empty,
            "Sewing Products need a dedicated Article collection independent of legacy fabric links.");
    }

    [Test]
    public void Product_adds_article_and_rejects_duplicate_sku()
    {
        var product = CreateProduct();
        product.AddArticle(SewingArticle.Create(ArticleId.Create(0), product.Id, " 00246 ").Value, DateTimeOffset.UtcNow);
        Assert.That(product.Articles.Single().SaleSku, Is.EqualTo("00246"));
        Assert.That(product.AddArticle(SewingArticle.Create(ArticleId.Create(0), product.Id, "00246").Value, DateTimeOffset.UtcNow).IsFailure, Is.True);
        Assert.That(product.Articles, Has.Count.EqualTo(1));
    }

    [Test]
    public void Composition_replacement_is_atomic_when_scope_conflicts()
    {
        var product = CreateProduct();
        var article = SewingArticle.Create(ArticleId.Create(1), product.Id, "00246").Value;
        product.AddArticle(article, DateTimeOffset.UtcNow);
        var shared = SewingMaterialLine.Create(CompositionLineId.Create(1), product.Id, null, SewingMaterialKind.Fabric,
            FabricReferenceId.Create(10), null, null, 3.6m, SewingMaterialUnit.Metre, 0).Value;
        product.ReplaceComposition([shared], [], DateTimeOffset.UtcNow);
        var specific = SewingMaterialLine.Create(CompositionLineId.Create(2), product.Id, article.Id, SewingMaterialKind.Fabric,
            FabricReferenceId.Create(10), null, null, 2m, SewingMaterialUnit.Metre, 0).Value;
        Assert.That(product.ReplaceComposition([shared, specific], [], DateTimeOffset.UtcNow).IsFailure, Is.True);
        Assert.That(product.MaterialLines, Has.Count.EqualTo(1));
        Assert.That(product.MaterialLines.Single().Id, Is.EqualTo(shared.Id));
    }

    [Test]
    public void Variant_has_only_its_one_fabric_line()
    {
        var product = CreateProduct();
        var article = SewingArticle.Create(ArticleId.Create(4), product.Id, "00246").Value;
        product.AddArticle(article, DateTimeOffset.UtcNow);
        var accessory = SewingMaterialLine.Create(CompositionLineId.Create(1), product.Id, article.Id,
            SewingMaterialKind.Accessory, null, AccessoryReferenceId.Create(12), null, 2m, SewingMaterialUnit.Piece, 0).Value;
        var first = SewingMaterialLine.Create(CompositionLineId.Create(2), product.Id, article.Id,
            SewingMaterialKind.Fabric, FabricReferenceId.Create(10), null, null, 2m, SewingMaterialUnit.Metre, 0).Value;
        var second = SewingMaterialLine.Create(CompositionLineId.Create(3), product.Id, article.Id,
            SewingMaterialKind.Fabric, FabricReferenceId.Create(11), null, null, 2m, SewingMaterialUnit.Metre, 1).Value;

        Assert.That(product.ReplaceComposition([accessory], [], DateTimeOffset.UtcNow).IsFailure, Is.True);
        Assert.That(product.ReplaceComposition([first, second], [], DateTimeOffset.UtcNow).IsFailure, Is.True);

        product.SetArticleFabric(article.Id, FabricReferenceId.Create(11), 1.75m, DateTimeOffset.UtcNow);
        product.SetArticleFabric(article.Id, FabricReferenceId.Create(10), 3.6m, DateTimeOffset.UtcNow);
        var line = product.MaterialLines.Single();
        Assert.That((line.FabricId, line.Quantity, line.Unit),
            Is.EqualTo(((FabricReferenceId?)FabricReferenceId.Create(10), 3.6m, SewingMaterialUnit.Metre)));
    }

    [Test]
    public void Variant_sku_change_rejects_a_duplicate_and_removal_drops_its_fabric()
    {
        var product = CreateProduct();
        var first = SewingArticle.Create(ArticleId.Create(4), product.Id, "246").Value;
        var second = SewingArticle.Create(ArticleId.Create(5), product.Id, "247").Value;
        product.AddArticle(first, DateTimeOffset.UtcNow);
        product.AddArticle(second, DateTimeOffset.UtcNow);
        product.SetArticleFabric(second.Id, FabricReferenceId.Create(10), 3.6m, DateTimeOffset.UtcNow);

        Assert.That(product.UpdateArticle(first.Id, "247", true, DateTimeOffset.UtcNow).IsFailure, Is.True);
        var version = product.CompositionVersion;
        product.UpdateArticle(first.Id, " 248 ", false, DateTimeOffset.UtcNow);
        Assert.That((first.SaleSku, first.IsActive, product.CompositionVersion), Is.EqualTo(("248", false, version + 1)));

        product.RemoveArticle(second.Id, DateTimeOffset.UtcNow);
        Assert.That(product.Articles.Select(x => x.SaleSku), Is.EqualTo(new[] { "248" }));
        Assert.That(product.MaterialLines, Is.Empty);
    }

    [Test]
    public void Operation_manual_minutes_must_be_positive()
    {
        var productId = ProductId.Create(101);
        Assert.That(SewingOperationLine.Create(
            CompositionLineId.Create(0), productId, null, OperationReferenceId.Create(5), 1m, 0m, 0).IsFailure, Is.True);
    }

    [Test]
    public void Effective_materials_include_model_lines_and_selected_article_lines_in_order()
    {
        var product = CreateProduct();
        var article = SewingArticle.Create(ArticleId.Create(4), product.Id, "00246").Value;
        product.AddArticle(article, DateTimeOffset.UtcNow);
        var shared = SewingMaterialLine.Create(CompositionLineId.Create(1), product.Id, null, SewingMaterialKind.Accessory,
            null, AccessoryReferenceId.Create(12), null, 2m, SewingMaterialUnit.Piece, 1).Value;
        var articleLine = SewingMaterialLine.Create(CompositionLineId.Create(2), product.Id, article.Id, SewingMaterialKind.Fabric,
            FabricReferenceId.Create(11), null, null, 3.6m, SewingMaterialUnit.Metre, 0).Value;
        product.ReplaceComposition([shared, articleLine], [], DateTimeOffset.UtcNow);

        Assert.That(product.GetEffectiveMaterials(article.Id).Value.Select(x => x.Id), Is.EqualTo(new[] { articleLine.Id, shared.Id }));
        Assert.That(product.GetEffectiveMaterials().Value.Select(x => x.Id), Is.EqualTo(new[] { shared.Id }));
    }

    [Test]
    public void Readiness_reports_missing_composition_blockers_without_review()
    {
        var product = CreateProduct();
        var article = SewingArticle.Create(ArticleId.Create(9), product.Id, "SKU-9").Value;
        product.AddArticle(article, DateTimeOffset.UtcNow);
        var operation = SewingOperationLine.Create(CompositionLineId.Create(3), product.Id, null, OperationReferenceId.Create(3),
            1m, null, 0).Value;
        product.ReplaceComposition([], [operation], DateTimeOffset.UtcNow);

        var blockers = product.GetCalculationReadinessBlockers(article.Id);
        Assert.That(blockers, Has.None.Contains("reviewed"));
        Assert.That(blockers, Has.Some.Contains("No effective fabric"));

        var projectedBlockers = SewingCompositionReadiness.Evaluate(article.IsActive,
            product.GetEffectiveMaterials(article.Id).Value.Select(material =>
                new MaterialReadinessFact(material.Id, material.Kind)),
            product.OperationLines.Select(line =>
                new OperationReadinessFact(line.Id)));
        Assert.That(projectedBlockers, Is.EqualTo(blockers));
    }

    [Test]
    public void Import_preserves_metre_accessories_and_repeated_operation_times()
    {
        var product = CreateProduct();
        var material = SewingMaterialLine.Create(CompositionLineId.Create(1), product.Id, null,
            SewingMaterialKind.Accessory, null, AccessoryReferenceId.Create(2), null,
            1.9m, SewingMaterialUnit.Metre, 0).Value;
        var first = SewingOperationLine.Create(CompositionLineId.Create(2), product.Id, null,
            OperationReferenceId.Create(3), 1m, 20m, 0).Value;
        var second = SewingOperationLine.Create(CompositionLineId.Create(3), product.Id, null,
            OperationReferenceId.Create(3), 1m, 86m, 1).Value;

        product.ReplaceComposition([material], [first, second], DateTimeOffset.UtcNow);

        Assert.That(product.MaterialLines.Single().Unit, Is.EqualTo(SewingMaterialUnit.Metre));
        Assert.That(product.OperationLines.Select(x => x.ManualMinutes), Is.EqualTo(new decimal?[] { 20m, 86m }));
    }

    private static Product CreateProduct() => Product.Create(ProductId.Create(101), "Model", "Model RU",
        ProductSlug.Create("model").Value, ProductType.Sewing, DateTimeOffset.UtcNow).Value;
}
