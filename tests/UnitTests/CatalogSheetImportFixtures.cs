using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Contracts.SlugGeneration;
using Catalog.Application.Features.Product.SheetImport;
using Catalog.Application.Features.Product.SheetImport.DTOs;
using Catalog.Application.Features.Product.SheetImport.Planning;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;

namespace UnitTests;

/// <summary>Sheet rows, Reference codes and stored products for the sheet product import tests (CAT-010).</summary>
internal static class CatalogSheetImportFixtures
{
    public static readonly DateTimeOffset Now = new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);
    public const string Rule = "Одяг ЗІЗ — АВ-Центр";

    public static ProductReferenceCodes References() => new(
        [new(23, "FAB-023"), new(59, "FAB-059")],
        [new(5, "FUR-005")],
        [new(18, "OP-018"), new(88, "OP-088")],
        [new(1, "АВ-Центр"), new(2, "3C-Маркет")],
        [new(10, "Sewing", "Одяг ЛІТО", true), new(20, "Ppe", "Одяг вологозахисний", true)]);

    public static SheetSewingRow Sewing(string code = "MOD-025", string name = "Костюм «Верде» (куртка + штани)")
        => new(2, code, name, "Одяг ЛІТО", "Літо", true, "https://brevi.com.ua/kostyum-verde",
            "Костюм із курткою та штанами.",
            [new("246", true), new("247", false)],
            [
                new(5, null, "Accessory", "FUR-005", 6, "Piece", "Спільна складова для всіх артикулів моделі"),
                new(6, "246", "Fabric", "FAB-059", 3.6m, "Metre", "Варіант для артикулу 246"),
                new(7, "247", "Fabric", "FAB-023", 3.4m, "Metre", null),
            ],
            [
                new(9, null, "OP-018", 1, null, null),
                new(10, null, "OP-088", 2, 7.3m, "Перший шов"),
                new(11, null, "OP-088", 1, null, "Другий шов"),
            ]);

    public static SheetPpeRow Ppe(string code = "3", string name = "Костюм ВОЛОГОЗАХИСНИЙ", string? rule = Rule,
        decimal? price = 518.64m, string supplier = "АВ-Центр")
        => new(2, code, name, "Одяг вологозахисний", "шт.", "https://brevi.com.ua/kostyum-vologozahysnyj", supplier,
            "https://avcentr.com.ua/ua/kostyum-carina", "CARINA", price, true, false, rule,
            "Вологозахисний костюм.", null, null);

    public static ImportSheetProductsCommand Command(IReadOnlyList<SheetSewingRow>? sewing = null,
        IReadOnlyList<SheetPpeRow>? ppe = null, bool dryRun = true, bool deleteMissing = true)
        => new(sewing ?? [Sewing()], ppe ?? [Ppe()], dryRun, deleteMissing, "manager-1");

    public static SheetImportContext Context(IReadOnlyList<Product>? products = null,
        IReadOnlyDictionary<int, PpeOfferVersion>? offers = null, IReadOnlySet<int>? calculatedProducts = null,
        IReadOnlySet<long>? calculatedArticles = null)
        => new(products ?? [], calculatedProducts ?? new HashSet<int>(), calculatedArticles ?? new HashSet<long>(),
            References(), new HashSet<string> { Rule }, offers ?? new Dictionary<int, PpeOfferVersion>(),
            new SlugGenerator(), Now);

    public static SheetImportPlan Plan(ImportSheetProductsCommand? command = null, SheetImportContext? context = null)
        => SheetImportPlanner.Plan(command ?? Command(), context ?? Context());

    /// <summary>A stored product exactly as the import would write the planned one (articles get IDs 1, 2, …).</summary>
    public static Product Stored(SheetPlannedProduct planned, int? id = null)
    {
        var product = Product.Create(id is null ? planned.Id : ProductId.Create(id.Value), planned.Draft.Name,
            planned.Draft.Name, ProductSlug.Create(planned.Slug), planned.Draft.Type, Now);
        var offset = (id ?? planned.Id.Value) * 100;
        SheetProductApplier.ApplyCard(product, planned.Draft, Now,
            index => ArticleId.Create(offset + index + 1));
        if (planned.Draft.Sewing is { } sewing)
            SheetProductApplier.ApplyComposition(product, sewing, Now);
        return product;
    }

    public static Product Legacy(int id, ProductType type, string name, string? code)
    {
        var product = Product.Create(ProductId.Create(id), name, name + " RU", ProductSlug.Create("legacy-" + id), type, Now);
        product.SetCode(code, Now);
        return product;
    }

    public static PpeOfferVersion Offer(int productId, decimal price, string? rule = Rule)
        => new(Guid.NewGuid(), productId, new PpeOfferValues(rule, price, null, null, "Картка ЗІЗ", "01.10.2026"),
            true, "manager", Now, Now);

    /// <summary>The name transliteration is not under test: every name gives «tovar».</summary>
    private sealed class SlugGenerator : IProductSlugGenerator
    {
        public string GenerateFromUkrainianName(string name) => "tovar";
    }
}
