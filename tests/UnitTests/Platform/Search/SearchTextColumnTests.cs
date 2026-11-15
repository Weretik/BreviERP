using BuildingBlocks.Infrastructure.Database;

namespace UnitTests.Platform.Search;

public sealed class SearchTextColumnTests
{
    [Test]
    public void The_column_lowers_and_joins_the_columns_with_immutable_functions()
    {
        Assert.That(SearchTextColumn.Sql("Name", "Code"), Is.EqualTo(
            "replace(replace(lower(coalesce(\"Name\", '') || ' ' || coalesce(\"Code\", '')), '’', ''''), 'ʼ', '''')"));
    }

    [Test]
    public void A_column_list_is_required()
    {
        Assert.Throws<ArgumentException>(() => SearchTextColumn.Sql());
    }
}
