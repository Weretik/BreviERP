using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace IntegrationTests;

public sealed class SewingCompositionPersistenceTests
{
    private static CatalogDbContext CreateContext() => new(
        new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql("Host=localhost;Database=model_only;Username=test;Password=test")
            .Options);

    [Test]
    public void Composition_model_has_generated_ids_global_sku_and_optimistic_version()
    {
        using var db = CreateContext();
        var model = db.GetService<IDesignTimeModel>().Model;
        var article = model.FindEntityType(typeof(SewingArticle))!;
        var product = model.FindEntityType(typeof(Product))!;

        Assert.Multiple(() =>
        {
            Assert.That(article.FindProperty(nameof(SewingArticle.Id))!.ValueGenerated, Is.EqualTo(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd));
            Assert.That(article.FindProperty(nameof(SewingArticle.SaleSku))!.GetMaxLength(), Is.EqualTo(80));
            Assert.That(article.GetIndexes().Any(i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual([nameof(SewingArticle.SaleSku)])), Is.True);
            Assert.That(product.FindProperty(nameof(Product.ModelCode))!.GetMaxLength(), Is.EqualTo(80));
            Assert.That(product.GetIndexes().Any(i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual([nameof(Product.ModelCode)])), Is.True);
            Assert.That(product.FindProperty(nameof(Product.CompositionVersion))!.IsConcurrencyToken, Is.True);
            Assert.That(product.FindNavigation(nameof(Product.Articles))!.IsEagerLoaded, Is.False);
            Assert.That(product.FindNavigation(nameof(Product.MaterialLines))!.IsEagerLoaded, Is.False);
            Assert.That(product.FindNavigation(nameof(Product.OperationLines))!.IsEagerLoaded, Is.False);
        });
    }

    [Test]
    public void Composition_lines_map_required_integrity_constraints_and_precision()
    {
        using var db = CreateContext();
        var model = db.GetService<IDesignTimeModel>().Model;
        var article = model.FindEntityType(typeof(SewingArticle))!;
        var material = model.FindEntityType(typeof(SewingMaterialLine))!;
        var operation = model.FindEntityType(typeof(SewingOperationLine))!;

        var compositeArticleForeignKey = material.GetForeignKeys().Single(f => f.PrincipalEntityType.ClrType == typeof(SewingArticle));
        Assert.Multiple(() =>
        {
            Assert.That(compositeArticleForeignKey.Properties.Select(p => p.Name), Is.EqualTo(new[] { nameof(SewingMaterialLine.ProductId), nameof(SewingMaterialLine.ArticleId) }));
            Assert.That(compositeArticleForeignKey.PrincipalKey.Properties.Select(p => p.Name), Is.EqualTo(new[] { nameof(SewingArticle.ProductId), nameof(SewingArticle.Id) }));
            Assert.That(article.GetKeys().Any(k => k.IsPrimaryKey() == false && k.Properties.Select(p => p.Name).SequenceEqual([nameof(SewingArticle.ProductId), nameof(SewingArticle.Id)])), Is.True);
            Assert.That(material.GetCheckConstraints().Select(c => c.Name), Does.Contain("CK_SewingMaterialLines_Quantity_Positive"));
            Assert.That(material.GetCheckConstraints().Select(c => c.Name), Does.Contain("CK_SewingMaterialLines_Reference"));
            Assert.That(operation.GetCheckConstraints().Select(c => c.Name), Does.Contain("CK_SewingOperationLines_Quantity_Positive"));
            Assert.That(operation.GetCheckConstraints().Select(c => c.Name), Does.Contain("CK_SewingOperationLines_ManualMinutes_Positive"));
            Assert.That(operation.GetCheckConstraints().Select(c => c.Name), Does.Contain("CK_SewingOperationLines_SharedOnly"));
            Assert.That(operation.GetCheckConstraints().Select(c => c.Name), Does.Contain("CK_SewingOperationLines_OperationId_Positive"));
            Assert.That(material.FindProperty(nameof(SewingMaterialLine.Quantity))!.GetPrecision(), Is.EqualTo(18));
            Assert.That(material.FindProperty(nameof(SewingMaterialLine.Quantity))!.GetScale(), Is.EqualTo(6));
            Assert.That(operation.FindProperty(nameof(SewingOperationLine.Quantity))!.GetPrecision(), Is.EqualTo(18));
            Assert.That(material.FindProperty(nameof(SewingMaterialLine.ReferenceKey)), Is.Null);
        });
    }

    [Test]
    public void Multiple_unsaved_composition_records_can_be_tracked_with_generated_ids()
    {
        using var db = CreateContext();
        var now = DateTimeOffset.UtcNow;
        var productId = ProductId.Create(7);
        var product = Product.Create(productId, "Model", "Модель", ProductSlug.Create("model"), ProductType.Sewing, now);
        product.AddArticle(SewingArticle.Create(default, productId, "SKU-1"), now);
        product.AddArticle(SewingArticle.Create(default, productId, "SKU-2"), now);
        product.ReplaceComposition(
        [
            SewingMaterialLine.Create(default, productId, null, SewingMaterialKind.OtherCost, null, null, "COST-1", 1m, SewingMaterialUnit.Service, false, 0),
            SewingMaterialLine.Create(default, productId, null, SewingMaterialKind.OtherCost, null, null, "COST-2", 1m, SewingMaterialUnit.Service, false, 1)
        ], [SewingOperationLine.Create(default, productId, null, OperationReferenceId.Create(1), 1m, null, 0, false)], now);

        Assert.DoesNotThrow(() => db.Products.Add(product));
        var articleIds = db.Entry(product).Collection(x => x.Articles).CurrentValue!
            .Select(x => db.Entry(x).Property(y => y.Id)).ToArray();
        var materialIds = db.Entry(product).Collection(x => x.MaterialLines).CurrentValue!
            .Select(x => db.Entry(x).Property(y => y.Id)).ToArray();
        var operationIds = db.Entry(product).Collection(x => x.OperationLines).CurrentValue!
            .Select(x => db.Entry(x).Property(y => y.Id)).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(articleIds.All(x => x.IsTemporary), Is.True);
            Assert.That(articleIds.Select(x => x.CurrentValue.Value).Distinct().Count(), Is.EqualTo(2));
            Assert.That(materialIds.All(x => x.IsTemporary), Is.True);
            Assert.That(materialIds.Select(x => x.CurrentValue.Value).Distinct().Count(), Is.EqualTo(2));
            Assert.That(operationIds.All(x => x.IsTemporary), Is.True);
            Assert.That(operationIds.Select(x => x.CurrentValue.Value).Distinct().Count(), Is.EqualTo(1));
        });
    }
}
