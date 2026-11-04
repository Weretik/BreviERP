using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Production.Application.Abstractions;
using Production.Application.Orders;
using Production.Application.Orders.QuoteSources;
using Production.Application.Orders.Tracking;
using Production.Application.Stock;
using Production.Application.Stock.Issues;
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

internal sealed class FakeProductionSupplierReader : IProductionSupplierReader
{
    public List<ProductionSupplier> Suppliers { get; } = [];

    public Task<ProductionSupplier?> FindAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(Suppliers.FirstOrDefault(x => x.Id == id));
}

/// <summary>CDN in memory: keys of stored and deleted files.</summary>
internal sealed class FakeProductionFileStorage : IProductionFileStorage
{
    public List<string> Stored { get; } = [];
    public List<string> Deleted { get; } = [];

    public Task<string> UploadAsync(Stream content, string storageKey, string contentType,
        CancellationToken cancellationToken)
    {
        Stored.Add(storageKey);
        return Task.FromResult($"https://cdn.test{storageKey}");
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        Deleted.Add(storageKey);
        return Task.CompletedTask;
    }
}

/// <summary>The bot in memory; <see cref="FailWith"/> makes the next sends fail.</summary>
internal sealed class FakeProductionTelegram : IProductionTelegram
{
    private long _nextMessageId = 100;

    public bool IsConfigured { get; set; } = true;
    public string? FailWith { get; set; }
    public List<(long ChatId, int? ThreadId, string Html)> Sent { get; } = [];
    public List<ProductionTelegramChat> Chats { get; } = [];

    public Task<long> SendAsync(long chatId, int? threadId, string html, CancellationToken cancellationToken)
    {
        if (FailWith is not null) throw new ProductionTelegramException(FailWith);
        Sent.Add((chatId, threadId, html));
        return Task.FromResult(_nextMessageId++);
    }

    public Task<IReadOnlyList<ProductionTelegramChat>> ListChatsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ProductionTelegramChat>>(Chats);
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
    public FakeProductionSupplierReader Suppliers { get; } = new();
    public FakeProductionFileStorage Files { get; } = new();
    public FakeProductionTelegram Telegram { get; } = new();
    public ProductionLinks Links { get; } = new("https://office.test");

    public QuoteItemResolver Resolver() => new(Db, Models, Fabrics);

    public OrderCards Cards() => new(Db, Telegram);

    public OrderTracking Tracking() => new(Db, Staff, Cards(), Time);

    public StockReservations Reservations() => new(Db);

    public StockRows StockRows() => new(Fabrics);

    public StockChanges StockChanges() => new(Db, Staff, Fabrics, Reservations(), StockRows(), Time);

    public ItemFabricCards ItemFabric() => new(Db, Reservations());

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
