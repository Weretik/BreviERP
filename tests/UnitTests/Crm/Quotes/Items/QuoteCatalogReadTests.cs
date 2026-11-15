using Catalog.Contracts.Products;
using Crm.Infrastructure.Adapters.Catalog;
using Reference.Contracts.Fabrics;

namespace UnitTests.Crm.Quotes.Items;

/// <summary>Opening, printing or repricing a КП reads all its products at once (database audit 2026-11-15, 1.1).</summary>
public sealed class QuoteCatalogReadTests
{
    [Test]
    public async Task Products_of_a_quote_are_read_in_one_call()
    {
        var catalog = new CountingCatalog(Product(1), Product(2), Product(3));
        var reader = new QuoteCatalogReader(catalog, new NoFabrics());

        var found = await reader.GetAsync([1, 2, 3, 99], CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(found.Keys, Is.EquivalentTo(new[] { 1, 2, 3 }), "a missing product is left out");
            Assert.That(catalog.ManyCalls, Is.EqualTo(1));
            Assert.That(catalog.OneCalls, Is.Zero, "no query per product");
        });
    }

    [Test]
    public async Task No_products_means_no_call()
    {
        var catalog = new CountingCatalog();

        var found = await new QuoteCatalogReader(catalog, new NoFabrics()).GetAsync([], CancellationToken.None);

        Assert.That((found.Count, catalog.ManyCalls), Is.EqualTo((0, 0)));
    }

    private static CatalogProduct Product(int id) => new(id, CatalogProductKind.Sewing, $"MOD-{id}", $"Модель {id}", null,
        null, true, null, null, null, [], [], null, null);

    private sealed class CountingCatalog(params CatalogProduct[] products) : ICatalogProductReader
    {
        public int ManyCalls { get; private set; }
        public int OneCalls { get; private set; }

        public Task<IReadOnlyDictionary<int, CatalogProduct>> FindManyAsync(IReadOnlyCollection<int> productIds,
            CancellationToken cancellationToken)
        {
            ManyCalls++;
            return Task.FromResult<IReadOnlyDictionary<int, CatalogProduct>>(
                products.Where(x => productIds.Contains(x.Id)).ToDictionary(x => x.Id));
        }

        public Task<CatalogProduct?> FindAsync(int productId, CancellationToken cancellationToken)
        {
            OneCalls++;
            return Task.FromResult(products.FirstOrDefault(x => x.Id == productId));
        }

        public Task<CatalogProductPage> PageAsync(CatalogProductKind kind, string? search, int page, int pageSize,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<CatalogProduct>> ListAsync(CatalogProductKind kind, string? search,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<CatalogPhoto>> PhotosAsync(int productId, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class NoFabrics : IFabricDirectory
    {
        public Task<IReadOnlyList<FabricInfo>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<FabricInfo>>([]);
    }
}
