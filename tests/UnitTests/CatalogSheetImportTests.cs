using Catalog.Application.Features.Product.SheetImport.DTOs;
using Catalog.Application.Features.Product.SheetImport.Planning;
using Catalog.Domain.Products.Enums;
using static UnitTests.CatalogSheetImportFixtures;

namespace UnitTests;

/// <summary>Planning of the sheet product import (CAT-010): matching, creates, updates, deletes and the report.</summary>
public sealed class CatalogSheetImportTests
{
    [Test]
    public void Empty_catalog_creates_every_row_with_next_ids_free_slugs_and_a_first_price()
    {
        var plan = Plan(context: Context([Legacy(40, ProductType.Sewing, "Стара модель", "MOD-001")]),
            command: Command(deleteMissing: false));

        Assert.Multiple(() =>
        {
            Assert.That(plan.Problems, Is.Empty);
            Assert.That(plan.Sewing, Is.EqualTo(new SheetImportCounts(1, 0, 0)));
            Assert.That(plan.Ppe, Is.EqualTo(new SheetImportCounts(1, 0, 0)));
            Assert.That(plan.Writes.Select(x => x.Id.Value), Is.EqualTo(new[] { 41, 42 }));
            Assert.That(plan.Writes.Select(x => x.Slug), Is.EqualTo(new[] { "tovar", "tovar-3" }));
            Assert.That(plan.Writes[0].Draft.Sewing!.Operations, Has.Count.EqualTo(3), "repeated OP-088 stays two lines");
            Assert.That(plan.Writes[1].Offer!.ExpectedVersionId, Is.Null);
            Assert.That(plan.Writes[1].Offer!.Values.Source, Is.EqualTo("Імпорт з таблиці"));
            Assert.That(plan.Writes[1].Offer!.Values.SourceDate, Is.EqualTo("09.09.2026"));
            Assert.That(plan.Writes[1].Offer!.Values.SupplierCost, Is.EqualTo(518.64m));
            Assert.That(plan.Deletes, Is.Empty);
        });
    }

    [Test]
    public void Rows_match_by_code_first_then_by_name_and_keep_their_ids()
    {
        var byCode = Legacy(300, ProductType.Sewing, "Стара назва", "mod-025");
        var byName = Legacy(120, ProductType.Ppe, "Костюм ВОЛОГОЗАХИСНИЙ", "17");
        var sameNameOtherType = Legacy(121, ProductType.Sewing, "Костюм ВОЛОГОЗАХИСНИЙ", null);

        var plan = Plan(context: Context([byCode, byName, sameNameOtherType]));

        Assert.Multiple(() =>
        {
            Assert.That(plan.Problems, Is.Empty);
            Assert.That(plan.Writes.Select(x => x.Existing), Is.EqualTo(new[] { byCode, byName }));
            Assert.That(plan.Writes.Select(x => x.Id.Value), Is.EqualTo(new[] { 300, 120 }));
            Assert.That(plan.Writes.Select(x => x.Slug), Is.EqualTo(new[] { "legacy-300", "legacy-120" }));
            Assert.That(plan.Sewing, Is.EqualTo(new SheetImportCounts(0, 1, 0)));
            Assert.That(plan.Ppe, Is.EqualTo(new SheetImportCounts(0, 1, 0)));
            Assert.That(plan.Deletes, Is.EqualTo(new[] { sameNameOtherType }));
        });
    }

    [Test]
    public void A_stored_product_equal_to_its_row_is_unchanged_and_not_written()
    {
        var first = Plan();
        var sewing = Stored(first.Writes[0], 300);
        var ppe = Stored(first.Writes[1], 120);

        var plan = Plan(context: Context([sewing, ppe], new Dictionary<int, Catalog.Application.Contracts.Pricing.Ppe.PpeOfferVersion>
        {
            [120] = Offer(120, 518.64m),
        }));

        Assert.Multiple(() =>
        {
            Assert.That(plan.Problems, Is.Empty);
            Assert.That(plan.Writes, Is.Empty);
            Assert.That(plan.Sewing, Is.EqualTo(new SheetImportCounts(0, 0, 1)));
            Assert.That(plan.Ppe, Is.EqualTo(new SheetImportCounts(0, 0, 1)));
        });
    }

    [Test]
    public void Price_version_is_appended_only_when_price_or_rule_changes()
    {
        var first = Plan();
        var ppe = Stored(first.Writes[1], 120);
        var current = Offer(120, 500m);
        var context = Context([Stored(first.Writes[0], 300), ppe],
            new Dictionary<int, Catalog.Application.Contracts.Pricing.Ppe.PpeOfferVersion> { [120] = current });

        var changedPrice = Plan(context: context);
        var noPrice = Plan(Command(ppe: [Ppe(price: null)]), context);

        Assert.Multiple(() =>
        {
            Assert.That(changedPrice.Writes.Single().Offer!.ExpectedVersionId, Is.EqualTo(current.Id));
            Assert.That(changedPrice.Writes.Single().Offer!.Values.SupplierCost, Is.EqualTo(518.64m));
            Assert.That(changedPrice.Ppe, Is.EqualTo(new SheetImportCounts(0, 1, 0)));
            Assert.That(noPrice.Writes.Single().Offer, Is.Null, "no supplier price, no version");
            Assert.That(noPrice.Warnings.Select(x => x.Message), Has.Some.Contains("Немає ціни постачальника"));
        });
    }

    [Test]
    public void Delete_missing_lists_products_outside_the_sheet_only_when_asked()
    {
        var old = Legacy(7, ProductType.Ppe, "Старий ЗІЗ", "999");

        var deleting = Plan(context: Context([old]));
        var keeping = Plan(Command(deleteMissing: false), Context([old]));

        Assert.Multiple(() =>
        {
            Assert.That(deleting.ToReport(false).ToDelete, Is.EqualTo(new[] { new SheetImportDeletion(7, "Ppe", "999", "Старий ЗІЗ") }));
            Assert.That(keeping.Deletes, Is.Empty);
        });
    }

    [Test]
    public void A_product_with_saved_calculations_cannot_be_deleted_or_lose_an_article()
    {
        var old = Legacy(7, ProductType.Sewing, "Стара модель", "MOD-900");
        var first = Plan();
        var stored = Stored(first.Writes[0], 300);
        stored.ReplaceArticles([Catalog.Domain.Products.Entities.SewingArticle.Create(
            Catalog.Domain.Products.ValueObjects.ArticleId.Create(77), stored.Id, "999")], Now);

        var plan = Plan(context: Context([old, stored], calculatedProducts: new HashSet<int> { 7 },
            calculatedArticles: new HashSet<long> { 77 }));

        Assert.That(plan.Problems.Select(x => x.Message), Has.Some.Contains("Стара модель").And.Some.Contains("Артикула 999"));
    }

    [Test]
    public void Unknown_codes_sections_and_suppliers_are_problems_reported_in_one_pass()
    {
        var sewing = Sewing() with
        {
            Section = "Одяг ОСІНЬ",
            Materials = [new(5, null, "Accessory", "FUR-999", 1, "Piece", true, null), new(6, "555", "Fabric", "FAB-023", 1, "Metre", true, null)],
            Operations = [new(9, null, "OP-999", 1, null, null), new(10, "246", "OP-018", 1, null, null)],
        };
        var ppe = Ppe(supplier: "Невідомий");

        var plan = Plan(Command([sewing], [ppe]));

        Assert.Multiple(() =>
        {
            Assert.That(plan.Problems.Select(x => x.Message), Has.Some.Contains("Одяг ОСІНЬ")
                .And.Some.Contains("FUR-999").And.Some.Contains("Артикула 555").And.Some.Contains("OP-999")
                .And.Some.Contains("тільки спільні").And.Some.Contains("Невідомий"));
            Assert.That(plan.Problems.Select(x => x.RowNumber), Is.SupersetOf(new int?[] { 5, 6, 9, 10, 2 }));
            Assert.That(plan.Writes, Is.Empty);
        });
    }

    [Test]
    public void Unknown_rule_is_a_warning_and_the_product_is_imported_without_it()
    {
        var plan = Plan(Command(ppe: [Ppe(rule: "Одяг ЗІЗ")]));

        Assert.Multiple(() =>
        {
            Assert.That(plan.Problems, Is.Empty);
            Assert.That(plan.Warnings.Single().Message, Does.Contain("«Одяг ЗІЗ» не знайдено"));
            Assert.That(plan.Writes[1].Draft.Ppe!.PricingRuleCode, Is.Null);
            Assert.That(plan.Writes[1].Offer!.Values.RuleCode, Is.Null);
        });
    }

    [Test]
    public void Supplier_typo_from_the_sheet_resolves_through_the_alias()
    {
        var plan = Plan(Command(ppe: [Ppe(supplier: " 3c-макркет ")]));

        Assert.That(plan.Writes[1].Draft.Ppe!.SupplierId.Value, Is.EqualTo(2));
    }

    [Test]
    public void Repeated_codes_names_and_sale_skus_in_the_body_are_problems()
    {
        var plan = Plan(Command(
            [Sewing(), Sewing(name: "Інша модель") with { RowNumber = 3 }],
            [Ppe("73", "Рукавички антивібраційні"), Ppe("74", "Рукавички антивібраційні") with { RowNumber = 3 }, Ppe("246", "ЗІЗ з чужим артикулом") with { RowNumber = 4 }]));

        Assert.That(plan.Problems.Select(x => x.Message), Has.Some.Contains("Код MOD-025 уже є в рядку 2")
            .And.Some.Contains("Назва «Рукавички антивібраційні» уже є в рядку 2")
            .And.Some.Contains("Артикул на сайті 246 уже використано"));
    }

    [Test]
    public void Checked_model_lines_are_reviewed_by_the_sheet_at_kyiv_midnight_of_the_check_date()
    {
        var verified = Stored(Plan().Writes[0]);
        var unchecked_ = Stored(Plan(Command([Sewing(verified: false, verifiedOn: null)])).Writes[0]);
        var noDate = Plan(Command([Sewing(verifiedOn: null)])).Writes[0].Draft;

        Assert.Multiple(() =>
        {
            Assert.That(verified.MaterialLines.Count + verified.OperationLines.Count, Is.EqualTo(6));
            Assert.That(verified.MaterialLines.All(x => x.IsReviewed && x.ReviewedBy == SheetProductApplier.Reviewer), Is.True);
            Assert.That(verified.OperationLines.All(x => x.IsReviewed && x.ReviewedAt == new DateTimeOffset(2026, 9, 7, 21, 0, 0, TimeSpan.Zero)), Is.True);
            // Npgsql writes timestamptz only from a UTC offset.
            Assert.That(verified.MaterialLines.Concat<object>(verified.OperationLines).Select(x => x switch
            {
                Catalog.Domain.Products.Entities.SewingMaterialLine m => m.ReviewedAt!.Value.Offset,
                Catalog.Domain.Products.Entities.SewingOperationLine o => o.ReviewedAt!.Value.Offset,
                _ => TimeSpan.MaxValue
            }), Is.All.EqualTo(TimeSpan.Zero));
            Assert.That(verified.GetCalculationReadinessBlockers(verified.Articles.First().Id), Is.Empty);
            Assert.That(unchecked_.OperationLines.Any(x => x.IsReviewed), Is.False);
            Assert.That(unchecked_.GetCalculationReadinessBlockers(), Is.Not.Empty);
            Assert.That(noDate.Overview.VerifiedOn, Is.EqualTo("03.11.2026"));
            Assert.That(noDate.Sewing!.ReviewedAt, Is.EqualTo(new DateTimeOffset(2026, 11, 2, 22, 0, 0, TimeSpan.Zero)));
        });
    }

    [Test]
    public void A_name_kept_by_a_product_outside_the_sheet_is_a_problem_without_delete_missing()
    {
        var other = Legacy(9, ProductType.Sewing, "Костюм ВОЛОГОЗАХИСНИЙ", "MOD-300");

        var plan = Plan(Command(deleteMissing: false), Context([other]));

        Assert.That(plan.Problems.Single().Message, Does.Contain("Назву «Костюм ВОЛОГОЗАХИСНИЙ»"));
    }

    [Test]
    public void Domain_rule_failures_become_problems_of_the_row()
    {
        var plan = Plan(Command([Sewing() with { CurrentSiteUrl = "brevi.com.ua/kostyum" }]));

        Assert.That(plan.Problems.Single().Message, Does.StartWith("Дані не пройшли перевірку"));
    }
}
