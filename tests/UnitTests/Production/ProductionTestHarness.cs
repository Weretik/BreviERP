using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Production.Application.Abstractions;
using Production.Application.Colors;
using Production.Application.Contracts;
using Production.Application.Orders;
using Production.Application.Orders.Embroidery;
using Production.Application.Orders.QuoteSources;
using Production.Application.Orders.Tracking;
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

    public List<ProductionModelComposition> Compositions { get; } = [];

    public Task<IReadOnlyList<ProductionModelComposition>> ListCompositionsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ProductionModelComposition>>(Compositions);
}

/// <summary>КП by id; a manager sees only the КП they are responsible for, like the Crm query.</summary>
internal sealed class FakeProductionQuoteReader : IProductionQuoteReader
{
    public List<ProductionQuote> Quotes { get; } = [];

    public Task<IReadOnlyList<ProductionQuoteSummary>> ListForWorkshopAsync(ProductionQuoteReaderActor actor, string? search,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ProductionQuoteSummary>>(Quotes
            .Where(x => x.ForWorkshop && (actor.CanSeeAll || x.ResponsibleUserId == actor.UserId))
            .Select(x => new ProductionQuoteSummary(x.Id, x.Number, x.ClientName, x.ResponsibleName, null)).ToList());

    public Task<ProductionQuote?> ReadAsync(ProductionQuoteReaderActor actor, long quoteId,
        CancellationToken cancellationToken) =>
        Task.FromResult(Quotes.FirstOrDefault(x => x.Id == quoteId && (actor.CanSeeAll || x.ResponsibleUserId == actor.UserId)));
}

/// <summary>Saved «Ціна вишивки» calculations by id.</summary>
internal sealed class FakeProductionEmbroideryReader : IProductionEmbroideryReader
{
    public Dictionary<long, ProductionEmbroideryResponse> Calculations { get; } = [];

    public Task<IReadOnlyList<ProductionEmbroideryChoiceResponse>> SearchAsync(int userId, string? search,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ProductionEmbroideryChoiceResponse>>(Calculations.Values
            .Where(x => search is null || x.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || x.Number == search)
            .Select(x => x.ToChoice()).ToList());

    public Task<IReadOnlyDictionary<long, ProductionEmbroideryResponse>> ReadAsync(int userId,
        IReadOnlyCollection<long> ids, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyDictionary<long, ProductionEmbroideryResponse>>(Calculations
            .Where(x => ids.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value));
}

internal sealed class FakeProductionFabricReader : IProductionFabricReader
{
    public List<ProductionFabric> Fabrics { get; } = [];

    public Task<IReadOnlyList<ProductionFabric>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ProductionFabric>>(Fabrics);
}

internal sealed class FakeProductionColorReader : IProductionColorReader
{
    public List<string> Colors { get; } = ["Темно-сірий", "Червоний", "Жовтий", "Сірий"];

    public Task<IReadOnlyList<string>> ListActiveAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<string>>(Colors);
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

/// <summary>An empty «Склад тканин»; a test that needs the stock sets the Warehouse harness instead.</summary>
internal sealed class EmptyProductionStockReader : IProductionStockReader
{
    public Task<IReadOnlyList<ProductionStockIssue>> IssuesAsync(DateOnly from, DateOnly to,
        CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<ProductionStockIssue>>([]);

    public Task<IReadOnlyList<ProductionStockBalance>> BalancesAsync(DateOnly before,
        CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<ProductionStockBalance>>([]);
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
    public FakeProductionEmbroideryReader Embroideries { get; } = new();
    public FakeProductionColorReader ColorList { get; } = new();
    public FakeProductionStaffReader Staff { get; } = new();
    public IProductionStockReader Stock { get; set; } = new EmptyProductionStockReader();

    /// <summary>Harnesses of other modules that share this test; disposed with it.</summary>
    public List<IDisposable> Attached { get; } = [];
    public FakeProductionFileStorage Files { get; } = new();
    public FakeProductionTelegram Telegram { get; } = new();
    public FakePayrollParameterReader Parameters { get; } = new();
    public ProductionLinks Links { get; } = new("https://office.test");

    public QuoteItemResolver Resolver() => new(Db, Models, Fabrics);

    public EmbroideryLinks EmbroideryLinks() => new(Embroideries);

    public ProductionColors Colors() => new(ColorList);

    public OrderCards Cards() => new(Db, Telegram, EmbroideryLinks());

    public OrderTracking Tracking() => new(Db, Staff, Cards(), Time);

    public void Dispose()
    {
        foreach (var attached in Attached) attached.Dispose();
        Db.Dispose();
        _connection.Dispose();
    }
}
