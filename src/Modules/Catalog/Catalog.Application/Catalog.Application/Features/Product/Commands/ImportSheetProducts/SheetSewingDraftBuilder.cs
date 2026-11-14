using Catalog.Domain.Products;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.Sewing.Enums;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Application.Features.Product.Commands.ImportSheetProducts;

/// <summary>
/// Resolves one «Товари» model with its lines into a draft and reports every unknown code or wrong value of it.
/// </summary>
public sealed class SheetSewingDraftBuilder(SheetReferenceResolver references, SheetImportIssueLog log)
{
    private const string Scope = "sewing";

    /// <returns>Null when the row has a problem.</returns>
    public SheetProductDraft? Build(SheetSewingRow row)
    {
        var before = log.Problems.Count;
        var code = SheetReferenceResolver.NormalizeCode(row.Code);
        void Problem(string message) => log.Problem(Scope, row.RowNumber, code, message);

        var sectionId = references.Section(ProductType.Sewing, row.Section);
        if (sectionId is null)
            Problem(string.IsNullOrWhiteSpace(row.Section)
                ? "Немає розділу."
                : $"Розділ «{row.Section.Trim()}» не знайдено серед швейних розділів довідника «Типи і розділи».");

        var skus = new HashSet<string>(StringComparer.Ordinal);
        foreach (var article in row.Articles)
        {
            if (string.IsNullOrWhiteSpace(article.SaleSku))
                Problem("Артикул без номера.");
            else if (!skus.Add(article.SaleSku.Trim()))
                Problem($"Артикул {article.SaleSku.Trim()} повторюється в моделі.");
        }

        var materials = new List<SewingMaterialDraft>();
        foreach (var line in row.Materials)
        {
            var material = Material(line, skus, message => log.Problem(Scope, line.RowNumber, code, message));
            if (material is not null) materials.Add(material);
        }

        var operations = new List<SewingOperationDraft>();
        foreach (var line in row.Operations)
        {
            void LineProblem(string message) => log.Problem(Scope, line.RowNumber, code, message);
            if (!string.IsNullOrWhiteSpace(line.Article))
                LineProblem($"Операція прив'язана до артикула {line.Article.Trim()}: операції бувають тільки спільні для моделі.");
            var operationId = references.Operation(line.Code);
            if (operationId is null)
                LineProblem(string.IsNullOrWhiteSpace(line.Code)
                    ? "Немає коду операції."
                    : $"Операцію {line.Code.Trim()} не знайдено в довіднику «Операції».");
            if (line.Quantity is not > 0)
                LineProblem("Кількість операції має бути більша за 0.");
            if (line.ManualMinutes is <= 0)
                LineProblem("Час вручну має бути більший за 0 або порожній.");
            if (operationId is not null && line.Quantity is > 0)
                operations.Add(new SewingOperationDraft(operationId.Value, line.Quantity.Value, line.ManualMinutes, Note(line.Comment)));
        }

        if (log.Problems.Count > before)
            return null;
        var articles = row.Articles.Select(x => x with { SaleSku = x.SaleSku.Trim() }).ToList();
        if (materials.Any(x => x.ArticleSku is not null && x.Kind != SewingMaterialKind.Fabric))
        {
            Problem("У артикула може бути тільки його тканина; фурнітура спільна для моделі.");
            return null;
        }
        materials = FabricPerVariant(materials, articles.Select(x => x.SaleSku).ToList());
        return new SheetProductDraft(row.RowNumber, ProductType.Sewing, code, row.Name.Trim(),
            new ProductOverviewDetails(sectionId, Trim(row.Season), row.IsActive,
                Trim(row.CurrentSiteUrl), Trim(row.QuoteDescription)),
            null, new SewingCompositionDraft(articles, materials, operations));
    }

    /// <summary>
    /// A fabric shared by the whole model goes to every variant that has no own fabric (CAT-015): the fabric is part
    /// of the variant, everything else is shared.
    /// </summary>
    internal static List<SewingMaterialDraft> FabricPerVariant(List<SewingMaterialDraft> materials, IReadOnlyList<string> skus)
    {
        var shared = materials.Where(x => x.ArticleSku is null && x.Kind == SewingMaterialKind.Fabric).ToList();
        if (shared.Count != 1 || skus.Count == 0)
            return materials;
        var withFabric = materials.Where(x => x.ArticleSku is not null).Select(x => x.ArticleSku!).ToHashSet(StringComparer.Ordinal);
        if (skus.Any(withFabric.Contains))
            return materials;
        return materials.Where(x => x != shared[0])
            .Concat(skus.Select(sku => shared[0] with { ArticleSku = sku })).ToList();
    }

    private SewingMaterialDraft? Material(SheetSewingMaterialRow line, HashSet<string> skus, Action<string> problem)
    {
        var before = log.Problems.Count;
        var article = Trim(line.Article);
        if (article is not null && !skus.Contains(article))
            problem($"Артикула {article} немає серед артикулів моделі.");
        SewingMaterialKind? kind = line.Kind switch
        {
            "Fabric" => SewingMaterialKind.Fabric,
            "Accessory" => SewingMaterialKind.Accessory,
            _ => null,
        };
        int? referenceId = null;
        if (kind is null)
            problem($"Вид матеріалу «{line.Kind}»: має бути Fabric або Accessory.");
        else
        {
            referenceId = kind == SewingMaterialKind.Fabric ? references.Fabric(line.Code) : references.Accessory(line.Code);
            if (referenceId is null)
                problem(string.IsNullOrWhiteSpace(line.Code)
                    ? "Немає коду матеріалу."
                    : kind == SewingMaterialKind.Fabric
                        ? $"Тканину {line.Code.Trim()} не знайдено в довіднику «Тканини»."
                        : $"Фурнітуру {line.Code.Trim()} не знайдено в довіднику «Фурнітура».");
        }
        SewingMaterialUnit? unit = line.Unit switch
        {
            "Metre" => SewingMaterialUnit.Metre,
            "Piece" => SewingMaterialUnit.Piece,
            _ => null,
        };
        if (unit is null)
            problem($"Одиниця «{line.Unit}»: має бути Metre або Piece.");
        else if (kind == SewingMaterialKind.Fabric && unit != SewingMaterialUnit.Metre)
            problem("Тканина рахується тільки в метрах.");
        if (line.Quantity is not > 0)
            problem("Норма матеріалу має бути більша за 0.");

        return log.Problems.Count > before
            ? null
            : new SewingMaterialDraft(article, kind!.Value, referenceId!.Value, line.Quantity!.Value, unit!.Value,
                Note(line.Comment));
    }

    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>The sheet repeats <see cref="SharedLineNote"/> on every shared line; it carries no information.</summary>
    private static string? Note(string? value) => Trim(value) is { } note && note != SharedLineNote ? note : null;

    public const string SharedLineNote = "Спільна складова для всіх артикулів моделі";
}
