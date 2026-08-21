using Ardalis.Result;
using Catalog.Api.Contracts.Products;
using Catalog.Domain.Products.Enums;

namespace UnitTests;

public sealed class GetProductListRequestMapperTests
{
    [Test]
    public void Maps_public_query_names_to_application_query()
    {
        var result = GetProductListRequestMapper.Map(new(
            Page: 2,
            PageSize: 50,
            Search: "jacket",
            Type: "Ppe",
            CategoryId: 7,
            SortBy: "updatedAt",
            SortDirection: "desc"));

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(ResultStatus.Ok));
            Assert.That(result.Value.Type, Is.EqualTo(ProductType.Ppe));
            Assert.That(result.Value.Descending, Is.True);
            Assert.That(result.Value.SortBy, Is.EqualTo("updatedAt"));
        });
    }

    [Test]
    public void Rejects_noncanonical_type_and_sort_direction()
    {
        var result = GetProductListRequestMapper.Map(new(Type: "ppe", SortDirection: "descending"));

        Assert.That(result.ValidationErrors.Select(x => x.Identifier), Is.EquivalentTo(["type", "sortDirection"]));
    }
}
