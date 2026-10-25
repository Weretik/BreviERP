using Ardalis.Result;
using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Features.Pricing.Sewing.Publications.GetProducts;
using Moq;

namespace UnitTests;

public sealed class PublishedSewingProductsQueryTests
{
    [Test]
    public async Task Invalid_or_unbounded_product_sets_are_rejected_without_reading()
    {
        var reader = new Mock<ISewingPublishedPriceReader>();
        var handler = new GetPublishedSewingProductsQueryHandler(reader.Object);
        foreach (var ids in new int[][] { [], [0], [1, 1], Enumerable.Range(1, 101).ToArray() })
            Assert.That((await handler.Handle(new(ids), CancellationToken.None)).Status, Is.EqualTo(ResultStatus.Invalid));
        reader.VerifyNoOtherCalls();
    }
}
