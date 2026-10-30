using Crm.Application.CustomSewing.GetSuggestions.Contracts;
using Crm.Application.CustomSewing.GetSuggestions;
namespace UnitTests.Crm.CustomSewing;

public sealed class SewingSuggestionRulesTests
{
    private static SuggestionInput Model(string code, decimal quantity = 1, bool approved = true)
        => new(code, "Куртка", "winter", approved, [new("accessory", 7, "Pieces", quantity, null)]);
    private static SewingSuggestions Aggregate(params SuggestionInput[] models)
        => SuggestionNormAggregator.Aggregate("Куртка", "winter", new(models, false));
    [TestCase(" Куртка   зимова ", "Куртка")]
    [TestCase("КУРТКА", "Куртка")]
    [TestCase("Напівкомбінезон зимовий", "Напівкомбінезон")]
    [TestCase("Куртка-пілот", null)]
    [TestCase("Курткаш", null)]
    [TestCase("Зимова Куртка", null)]
    [TestCase("Unknown", null)]
    public void Only_confirmed_legacy_name_boundaries_define_type(string name, string? expected)
        => Assert.That(LegacySewingSuggestionType.FromModelName(name), Is.EqualTo(expected));
    [Test]
    public void Identical_articles_count_once_per_unique_model_and_use_upper_median()
    {
        var result = Aggregate(Model("A", 1), Model("A", 1), Model("B", 3));
        Assert.That(result.SampleSize, Is.EqualTo(2));
        Assert.That(result.Norms.Single().SampleSize, Is.EqualTo(2));
        Assert.That(result.Norms.Single().Quantity, Is.EqualTo(3));
        Assert.That(result.Norms.Single().VaryingNorm, Is.True);
    }
    [Test]
    public void Ambiguous_or_unapproved_or_missing_references_exclude_the_entire_model()
    {
        var result = Aggregate(Model("A"), Model("A", 2), Model("B"), Model("B", approved: false),
            Model("C") with { Selections = [new("accessory", 0, "Pieces", 1, null)] }, Model("D"), Model("E"));
        Assert.That(result.SampleSize, Is.EqualTo(2));
        Assert.That(result.Norms.Single().SampleSize, Is.EqualTo(2));
    }
    [Test]
    public void Single_model_and_less_than_seventy_percent_coverage_do_not_suggest_norms()
    {
        Assert.That(Aggregate(Model("A")).Norms, Is.Empty);
        var other = Model("C") with { Selections = [new("accessory", 8, "Pieces", 1, null)] };
        Assert.That(Aggregate(Model("A"), Model("B"), other).Norms, Is.Empty);
    }
    [Test]
    public void Exact_season_and_type_are_required_and_no_price_fields_are_exposed()
    {
        Assert.That(Aggregate(Model("A"), Model("B") with { Season = "summer" }).Norms, Is.Empty);
        var serialized = System.Text.Json.JsonSerializer.Serialize(Aggregate(Model("A"), Model("B")));
        Assert.That(serialized, Does.Not.Contain("Cost").And.Not.Contain("Price").And.Not.Contain("Supplier"));
        Assert.That(LegacySewingSuggestionType.SourceSeason("winter"), Is.EqualTo("Зима"));
        Assert.That(LegacySewingSuggestionType.SourceSeason("Winter"), Is.Null);
    }
}
