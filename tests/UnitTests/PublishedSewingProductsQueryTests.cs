using Ardalis.Result;
using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Features.Pricing.Sewing.Publications.GetProducts;
using Moq;

namespace UnitTests;

public sealed class PublishedSewingProductsQueryTests
{
    [Test]
    public void Invalid_or_unbounded_product_sets_are_rejected_by_the_validator()
    {
        var validator = new GetPublishedSewingProductsQueryValidator();
        foreach (var ids in new int[][] { [], [0], [1, 1], Enumerable.Range(1, 101).ToArray() })
            Assert.That(validator.Validate(new GetPublishedSewingProductsQuery(ids)).IsValid, Is.False);
    }
}
