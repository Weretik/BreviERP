using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Production.Application.Abstractions;
using Production.Application.Orders.QuoteSources;
using Production.Infrastructure.Persistence;

namespace UnitTests.Production;

internal sealed class FakeProductionModelReader : IProductionModelReader
{
    public List<ProductionModel> Models { get; } = [];

    public Dictionary<int, ProductionModelCosts> Costs { get; } = [];

    public Task<IReadOnlyList<ProductionModel>> ListSewingModelsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ProductionModel>>(Models);

    public Task<ProductionModelCosts?> ReadCostsAsync(int productId, CancellationToken cancellationToken) =>
        Task.FromResult(Costs.GetValueOrDefault(productId));
}

/// <summary>КП by id; a manager sees only the КП they are responsible for, like the Crm query.</summary>
internal sealed class FakeProductionQuoteReader : IProductionQuoteReader
{
    public List<ProductionQuote> Quotes { get; } = [];

    public Task<IReadOnlyList<ProductionQuoteSummary>> ListSentAsync(ProductionQuoteReaderActor actor, string? search,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ProductionQuoteSummary>>(Quotes
            .Where(x => x.IsSent && (actor.CanSeeAll || x.ResponsibleUserId == actor.UserId))
            .Select(x => new ProductionQuoteSummary(x.Id, x.Number, x.ClientName, x.ResponsibleName, null)).ToList());

    public Task<ProductionQuote?> ReadAsync(ProductionQuoteReaderActor actor, long quoteId,
        CancellationToken cancellationToken) =>
        Task.FromResult(Quotes.FirstOrDefault(x => x.Id == quoteId && (actor.CanSeeAll || x.ResponsibleUserId == actor.UserId)));
}

internal sealed class FakeProductionFabricReader : IProductionFabricReader
{
    public List<ProductionFabric> Fabrics { get; } = [];

    public Task<IReadOnlyList<ProductionFabric>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ProductionFabric>>(Fabrics);
}

internal sealed class FakeProductionStaffReader : IProductionStaffReader
{
    public List<ProductionStaffMember> Staff { get; } = [];

    public Task<ProductionStaffMember?> FindAsync(int userId, CancellationToken cancellationToken) =>
        Task.FromResult(Staff.FirstOrDefault(x => x.UserId == userId));

    public Task<ProductionStaffMember?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        Task.FromResult(Staff.FirstOrDefault(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase)));
}

internal sealed class FakePayrollParameterReader : IPayrollParameterReader
{
    public List<PayrollParameterValue> Values { get; } = [];

    public Task<IReadOnlyList<PayrollParameterValue>> ReadAsync(IReadOnlyCollection<string> codes, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<PayrollParameterValue>>(Values.Where(x => codes.Contains(x.Code)).ToList());
}

/// <summary>Production module on SQLite in memory with fake Catalog models.</summary>
internal sealed class ProductionTestHarness : IDisposable
{
    private readonly SqliteConnection _connection;

    public ProductionTestHarness()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        Db = new ProductionDbContext(new DbContextOptionsBuilder<ProductionDbContext>().UseSqlite(_connection).Options);
        Db.Database.EnsureCreated();
    }

    public ProductionDbContext Db { get; }
    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero));
    public FakeProductionModelReader Models { get; } = new();
    public FakeProductionQuoteReader Quotes { get; } = new();
    public FakeProductionFabricReader Fabrics { get; } = new();
    public FakeProductionStaffReader Staff { get; } = new();

    public QuoteItemResolver Resolver() => new(Db, Models, Fabrics);

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
