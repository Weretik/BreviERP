using System.Text.Json;
using Ardalis.Result;
using Crm.Application.CustomSewing.Production.Contracts;
using Crm.Application.CustomSewing.Production.Projection;
using Crm.Application.Quotes.Pricing;
using Crm.Domain.CustomSewing.Composition;
using Crm.Domain.CustomSewing.Identifiers;
using Crm.Domain.CustomSewing.Pricing;
using Crm.Domain.Quotes.Identifiers;
using Crm.Domain.Quotes.Items;
using Crm.Domain.Quotes.Pricing;
using Crm.Domain.Quotes.Variants;
using Crm.Domain.Quotes.Versions;

namespace UnitTests.Crm.CustomSewing.Production;

public sealed class ProductionPackageProjectionTests
{
    private static readonly Guid FileId = Guid.NewGuid();

    [Test]
    public void Manifest_contains_only_selected_pinned_production_data_and_wages()
    {
        var visible = Variant();
        var hidden = Variant(visible: false, sortOrder: 1);
        var version = QuoteVersion.Create(new QuoteId(1), 2, 1, "edited", "CUSTOMER_PRIVATE_SENTINEL",
            Guid.NewGuid(), Guid.NewGuid(), 7,
            [QuoteItem.Create(Guid.NewGuid(), 0, "PRIVATE_ITEM_TITLE", "PRIVATE_DESCRIPTION", null,
                "PRIVATE_QUOTE_NOTE", true, [visible, hidden])]);
        var source = Source();
        var result = Build(version, [visible.PublicId], [source]);
        Assert.That(result.IsSuccess, Is.True, string.Join(',', result.ValidationErrors.Select(x => x.ErrorMessage)));
        var manifest = result.Value;
        var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Multiple(() =>
        {
            Assert.That(manifest.SchemaVersion, Is.EqualTo(1));
            Assert.That(manifest.Kind, Is.EqualTo("brevi.production.v1"));
            Assert.That(manifest.Options, Has.Count.EqualTo(1));
            Assert.That(manifest.Options[0].CustomVersionPublicId, Is.EqualTo(source.Snapshot.VersionPublicId));
            Assert.That(manifest.Options[0].Fabrics[0].Quantity, Is.EqualTo(2.5m));
            Assert.That(manifest.Options[0].Operations[0].EffectiveMinutes, Is.EqualTo(12m));
            Assert.That(manifest.Options[0].Operations[0].ReferenceVersionId, Is.EqualTo(91));
            Assert.That(manifest.Options[0].WageBasis.MinuteCost, Is.EqualTo(3m));
            Assert.That(manifest.Options[0].WageBasis.Bonus, Is.EqualTo(7m));
            Assert.That(manifest.Files, Has.Count.EqualTo(1));
            Assert.That(manifest.Files[0].ArchivePath, Is.EqualTo($"files/{FileId:D}.png"));
            Assert.That(manifest.Options[0].ProductionFileIds, Is.EquivalentTo(new[] { FileId }));
            foreach (var sentinel in new[] { "CUSTOMER_PRIVATE_SENTINEL", "PRIVATE_ITEM_TITLE", "PRIVATE_QUOTE_NOTE",
                         "PRIVATE_DESCRIPTION", "PRIVATE_CUSTOM_NOTE", "PRIVATE_CLIENT_COMMENT", "PRIVATE_SOURCE_CODE",
                         "PRIVATE_BASIS_SENTINEL", "77777", "88888", "99999", "internalCost", "retail", "largeWholesale" })
                Assert.That(json, Does.Not.Contain(sentinel));
        });
    }

    [Test]
    public void Hidden_excluded_noncustom_and_missing_custom_selections_are_rejected()
    {
        var hidden = Variant(visible: false, sortOrder: 1);
        AssertInvalid(Build(Version(hidden), [hidden.PublicId], [Source()]), "production_variant_not_selected_or_custom");
        var visible = Variant();
        var excluded = QuoteVersion.Create(new QuoteId(1), 1, null, "created", "Клієнт", null, null, 7,
            [QuoteItem.Create(Guid.NewGuid(), 0, "Варіант", null, null, null, false, [visible])]);
        AssertInvalid(Build(excluded, [visible.PublicId], [Source()]), "production_variant_not_selected_or_custom");
        var catalog = QuoteVariant.Create(Guid.NewGuid(), 0, "Каталог", "sewing", 1, 2, true,
            Guid.NewGuid(), "private", Tiers());
        AssertInvalid(Build(Version(catalog), [catalog.PublicId], [Source()]), "production_variant_not_selected_or_custom");
        AssertInvalid(Build(Version(visible), [visible.PublicId], []), "production_custom_version_missing");
        AssertInvalid(Build(Version(visible), [Guid.NewGuid()], [Source()]), "production_variant_not_selected_or_custom");
    }

    [Test]
    public void Artwork_must_be_on_the_selected_immutable_production_file_allowlist()
    {
        var variant = Variant();
        var source = Source();
        var publicOnly = source with { Files = source.Files.Select(x => x with { Production = false }).ToArray() };
        AssertInvalid(Build(Version(variant), [variant.PublicId], [publicOnly]), "production_artwork_not_allowed");
        var wrongHash = source with { Files = [source.Files[0] with { Sha256 = "invalid" }] };
        AssertInvalid(Build(Version(variant), [variant.PublicId], [wrongHash]), "production_file_metadata_invalid");
        var wrongMime = source with { Files = [source.Files[0] with { ContentType = "text/html" }] };
        AssertInvalid(Build(Version(variant), [variant.PublicId], [wrongMime]), "production_file_metadata_invalid");
    }

    [Test]
    public void Operation_norm_comes_only_from_saved_basis_and_manual_override_remains_explicit()
    {
        var variant = Variant();
        var source = Source();
        var absent = source with { Snapshot = source.Snapshot with { Basis = source.Snapshot.Basis with { References = [] } } };
        AssertInvalid(Build(Version(variant), [variant.PublicId], [absent]), "production_operation_basis_missing_or_ambiguous");
        var duplicate = source with { Snapshot = source.Snapshot with { Basis = source.Snapshot.Basis with
            { References = [source.Snapshot.Basis.References[0], source.Snapshot.Basis.References[0]] } } };
        AssertInvalid(Build(Version(variant), [variant.PublicId], [duplicate]), "production_operation_basis_missing_or_ambiguous");
        var overridden = source with { Snapshot = source.Snapshot with { Configuration = source.Snapshot.Configuration with
            { Operations = [new CustomOperation(3, 2m, 9m)] } } };
        overridden = overridden with { Snapshot = overridden.Snapshot with { Basis = overridden.Snapshot.Basis with { Breakdown = overridden.Snapshot.Basis.Breakdown! with { TotalMinutes = 18m } } } };
        var result = Build(Version(variant), [variant.PublicId], [overridden]);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Options[0].Operations[0].StandardMinutes, Is.EqualTo(12m));
        Assert.That(result.Value.Options[0].Operations[0].ManualMinutes, Is.EqualTo(9m));
        Assert.That(result.Value.Options[0].Operations[0].EffectiveMinutes, Is.EqualTo(9m));
    }

    [Test]
    public void Output_is_copied_from_the_supplied_snapshot_not_mutable_master_collections()
    {
        var variant = Variant();
        var source = Source();
        var materials = new[] { new CustomMaterial(11, 2.5m, "м") };
        source = source with { Snapshot = source.Snapshot with { Configuration = source.Snapshot.Configuration with { Fabrics = materials } } };
        var result = Build(Version(variant), [variant.PublicId], [source]);
        materials[0] = new CustomMaterial(99, 99m, "MASTER_CHANGED");
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Options[0].Fabrics[0], Is.EqualTo(new ProductionMaterial(11, 2.5m, "м")));
    }

    [Test]
    public void Duplicate_selected_variant_and_conflicting_file_provenance_are_rejected()
    {
        var variant = Variant();
        AssertInvalid(Build(Version(variant), [variant.PublicId, variant.PublicId], [Source()]), "production_selection_invalid");
        var source = Source();
        source = source with { Files = [source.Files[0], source.Files[0] with { Length = 999 }] };
        AssertInvalid(Build(Version(variant), [variant.PublicId], [source]), "production_file_provenance_conflict");
    }

    private static Result<ProductionPackageManifest> Build(QuoteVersion version, IReadOnlyList<Guid> selected,
        IReadOnlyList<ProductionCustomVersionSource> sources) =>
        ProductionPackageProjectionBuilder.Build(Guid.NewGuid(), "10/10/2026", version, selected, sources);

    private static void AssertInvalid(Result<ProductionPackageManifest> result, string code)
    {
        Assert.That(result.Status, Is.EqualTo(ResultStatus.Invalid));
        Assert.That(result.ValidationErrors.Select(x => x.ErrorMessage), Does.Contain(code));
    }

    private static QuoteVariant Variant(bool visible = true, int sortOrder = 0) => QuoteVariant.Create(Guid.NewGuid(), sortOrder,
        "Варіант", "customSewing", null, null, visible, Guid.NewGuid(), "PRIVATE_BASIS_SENTINEL",
        Tiers(), new CustomVersionId(7));

    private static QuoteTier[] Tiers() => [QuoteTier.Create("retail", 150m, true),
        QuoteTier.Create("wholesale", 140m, true), QuoteTier.Create("largeWholesale", 130m, true)];

    private static QuoteVersion Version(QuoteVariant variant) => QuoteVersion.Create(new QuoteId(1), 1, null,
        "created", "Клієнт", null, null, 7,
        [QuoteItem.Create(Guid.NewGuid(), 0, "Позиція", null, null, null, true, [variant])]);

    private static ProductionCustomVersionSource Source()
    {
        var configuration = new CustomConfiguration("Куртка", "jacket", "summer", "PRIVATE_DESCRIPTION",
            "PRIVATE_CLIENT_COMMENT", "PRIVATE_CUSTOM_NOTE", [new CustomMaterial(11, 2.5m, "м")],
            [new CustomMaterial(12, 5m, "шт")], [new CustomOperation(3, 2m, null)],
            [new CustomApplication("print", "На спині", FileId, 99999m, 88888m, true)], 77777m, 6m);
        var breakdown = new CustomCostBreakdown(10m, 20m, 24m, 3m, 72m, 4m, 5m, 88888m,
            7m, 88888m, 77777m, 6m, 99999m, 88888m);
        var basis = new CustomPriceBasis(Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(15),
            "custom.v1", 41, [new CustomReferenceBasis("Operation:3", 91, 12m, "Minutes", "PRIVATE_SOURCE_CODE")],
            breakdown, 100m, 150m, 140m, 130m, []);
        return new ProductionCustomVersionSource(new CustomVersionId(7),
            new QuoteCustomVersionSnapshot(Guid.NewGuid(), Guid.NewGuid(), 2, configuration, basis),
            [new ProductionPinnedFile(FileId, "image/png", 123, new string('a', 64), true),
             new ProductionPinnedFile(Guid.NewGuid(), "application/pdf", 10, new string('b', 64), false)]);
    }
}
