using BuildingBlocks.Application;

namespace UnitTests.Platform.Search;

public sealed class SearchWordsTests
{
    [Test]
    public void Every_word_becomes_one_lower_case_pattern()
    {
        Assert.That(SearchWords.Patterns("  Халат  МОД-52 халат "), Is.EqualTo(new[] { "%халат%", "%мод-52%" }));
    }

    [Test]
    public void Wildcards_are_escaped_and_apostrophes_unified()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SearchWords.Patterns(@"100% a_b c\d"), Is.EqualTo(new[] { @"%100\%%", @"%a\_b%", @"%c\\d%" }));
            Assert.That(SearchWords.Patterns("Об’єм Обʼєм"), Is.EqualTo(new[] { "%об'єм%" }));
        });
    }

    [Test]
    public void In_memory_every_word_must_be_found_in_one_of_the_texts_in_any_order()
    {
        string?[] texts = ["MOD-052", "Куртка «Віол»", null, "Об’ємна"];
        Assert.Multiple(() =>
        {
            Assert.That(SearchWords.MatchesEveryWord("віол мод", texts), Is.False, "мод is not in MOD (Latin)");
            Assert.That(SearchWords.MatchesEveryWord("ВІОЛ mod-05 об'єм", texts), Is.True);
            Assert.That(SearchWords.MatchesEveryWord("куртка халат", texts), Is.False, "халат is missing");
            Assert.That(SearchWords.MatchesEveryWord("  ", texts), Is.True);
        });
    }

    [Test]
    public void Empty_text_gives_no_condition()
    {
        Assert.Multiple(() =>
        {
            Assert.That(SearchWords.Patterns(null), Is.Empty);
            Assert.That(SearchWords.Patterns("   "), Is.Empty);
        });
    }
}
