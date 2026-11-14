using Crm.Application.Features.Quotes.Queries.GetQuoteFabricFacts;
using Crm.Application.Abstractions.Quotes;
using Crm.Application.Abstractions.Quotes.Products;

namespace UnitTests.Crm.Quotes;

/// <summary>R-31: the manager line under a КП fabric comes from the current Reference fabric.</summary>
[TestFixture]
public sealed class QuoteFabricFactsTests
{
    [Test]
    public async Task Returns_each_asked_fabric_once_with_cleaned_texts()
    {
        var reader = new FakeReader(
            new QuoteFabricFacts(1, "Грета", " 53% бавовна ", "220 г/м²", 133m, 7, "Спецтекстиль"),
            new QuoteFabricFacts(2, "Ортон", null, " ", 98.5m, null, "-"),
            new QuoteFabricFacts(3, "Не питали", null, null, 1m, null, null));
        var handler = new GetQuoteFabricFactsHandler(reader);

        var result = await handler.Handle(new GetQuoteFabricFactsQuery([1, 2, 1, 99]), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Select(x => x.FabricId), Is.EqualTo(new[] { 1, 2 }));
        var greta = result.Value[0];
        Assert.That((greta.Composition, greta.Density, greta.PricePerMeter, greta.SupplierId, greta.SupplierName),
            Is.EqualTo(("53% бавовна", "220 г/м²", 133m, (int?)7, "Спецтекстиль")));
        var orton = result.Value[1];
        Assert.That((orton.Density, orton.SupplierName), Is.EqualTo(((string?)null, (string?)null)),
            "blank texts and the «-» of a missing supplier are not shown");
        Assert.That(reader.Asked, Is.EqualTo(new[] { 1, 2, 99 }));
    }

    [TestCase(0)]
    [TestCase(201)]
    public void Asks_for_one_to_two_hundred_fabrics(int count)
    {
        var validator = new GetQuoteFabricFactsValidator();

        var result = validator.Validate(new GetQuoteFabricFactsQuery(Enumerable.Range(1, count).ToArray()));

        Assert.That(result.IsValid, Is.False);
    }

    private sealed class FakeReader(params QuoteFabricFacts[] fabrics) : IQuoteFabricFactsReader
    {
        public IReadOnlyCollection<int> Asked { get; private set; } = [];

        public Task<IReadOnlyList<QuoteFabricFacts>> GetAsync(IReadOnlyCollection<int> fabricIds,
            CancellationToken cancellationToken)
        {
            Asked = fabricIds;
            return Task.FromResult<IReadOnlyList<QuoteFabricFacts>>(fabrics);
        }
    }
}
