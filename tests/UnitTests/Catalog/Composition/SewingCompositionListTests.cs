using Moq;
using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Reference;
using Catalog.Application.Features.Product.Queries.ListSewingCompositions;
using Catalog.Application.Features.Product.Shared.Sewing;
using Catalog.Application.Abstractions.Reference.Items;
using Catalog.Application.Abstractions.Sewing;
using Catalog.Application.Features.Product.Shared.Sewing.Views;

namespace UnitTests.Catalog.Composition;

/// <summary>All sewing compositions at once, for the workshop «Комплектація моделей».</summary>
public sealed class SewingCompositionListTests
{
    [Test]
    public async Task Lines_carry_reference_codes_and_names_with_variant_fabrics_first()
    {
        var store = new Mock<ISewingCompositionListReader>();
        store.Setup(x => x.ReadAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            new SewingCompositionListRow(7, "MOD-052",
                [new SewingArticleView(11, 7, "386", true, 0, null, null)],
                [
                    new SewingMaterialView(1, null, "Model", "Accessory", 30, null, 1.9m, "Metre", 0, null),
                    new SewingMaterialView(2, 11, "Article", "Fabric", 4, null, 2.5m, "Metre", 0, null),
                    new SewingMaterialView(3, null, "Model", "OtherCost", null, "Пакування", 1m, "Service", 1, "Пакет"),
                ]),
        ]);
        var names = new Mock<IProductReferenceReader>();
        names.Setup(x => x.GetSnapshotAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new ProductReferenceData(
            new Dictionary<int, ReferenceItem>(),
            new Dictionary<int, PricedReferenceItem> { [4] = new(4, "Тканина «Грета»", 107m) },
            new Dictionary<int, PricedReferenceItem> { [30] = new(30, "Підкладка поліестерова", 40m) },
            new Dictionary<int, OperationReferenceItem>(),
            new Dictionary<int, AdditionalReferenceItem>()));
        var codes = new Mock<IProductReferenceCodeReader>();
        codes.Setup(x => x.ReadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new ProductReferenceCodes(
            [new ReferenceCodeItem(4, "FAB-023")], [new ReferenceCodeItem(30, "FUR-021")], [], [], []));

        var result = await new ListSewingCompositionsHandler(store.Object, names.Object, codes.Object)
            .Handle(new ListSewingCompositionsQuery(), CancellationToken.None);

        var lines = result.Value.Single().Lines;
        Assert.That(lines.Select(x => (x.Kind, x.Article, x.Code, x.Name)), Is.EqualTo(new[]
        {
            ("Fabric", (string?)"386", (string?)"FAB-023", (string?)"Тканина «Грета»"),
            ("Accessory", null, "FUR-021", "Підкладка поліестерова"),
            ("OtherCost", null, "Пакування", null),
        }));
    }
}
