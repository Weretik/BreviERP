using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Purchasing.Application.Abstractions;
using Purchasing.Application.Register.Review;
using Purchasing.Application.Register.Sync;
using Purchasing.Domain.Files;
using Purchasing.Domain.Ingestion;
using Purchasing.Domain.Messages;
using Purchasing.Domain.Sources;
using Purchasing.Domain.Sync;
using Purchasing.Infrastructure.Persistence;

namespace UnitTests.Purchasing;

/// <summary>Sheet rows in A:N order with sensible defaults; FileCount follows the links unless given.</summary>
internal static class SheetRow
{
    public const string Dropbox = "https://www.dropbox.com/scl/fi/abc/invoice.pdf?rlkey=x&dl=0";

    public static string?[] Of(string uid, string chatId = "-1001", string messageId = "10", string text = "Рахунок від Текстиль",
        string dateTime = "05.11.2026 10:00:00", string fileLinks = Dropbox, string? fileCount = null, string status = "",
        string statusDate = "", string fromUserStatus = "") =>
    [
        uid, "БРЕВІ команда", chatId, messageId, dateTime, "Олена", "42", text, "document", fileLinks,
        fileCount ?? FileLinkRules.Extract(fileLinks).Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
        status, statusDate, fromUserStatus,
    ];
}

internal sealed class FakeSheetReader : IPurchaseSheetReader
{
    public const string SpreadsheetId = "sheet-1";
    public const long SheetId = 824424675;

    public List<IReadOnlyList<string?>> Rows { get; set; } = [];
    public Exception? Failure { get; set; }

    public Task<PurchaseSheetContent> ReadAsync(CancellationToken cancellationToken) =>
        Failure is not null ? Task.FromException<PurchaseSheetContent>(Failure)
            : Task.FromResult(new PurchaseSheetContent(SpreadsheetId, SheetId, Rows.ToList()));
}

internal sealed class FakeSupplierReader : IPurchasingSupplierReader
{
    public List<PurchasingSupplier> Suppliers { get; } =
    [
        new(7, "Текстиль-Контакт", true),
        new(8, "ФОП Бистров", true),
        new(9, "Старий постачальник", false),
    ];

    public Task<IReadOnlyList<PurchasingSupplier>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<PurchasingSupplier>>(Suppliers.ToList());
}

/// <summary>The real module DbContext on in-memory SQLite; the PostgreSQL advisory lock becomes a flag.</summary>
internal sealed class SqlitePurchasingDb(PurchasingDbContext db) : IPurchasingDbContext
{
    public bool LockHeld { get; set; }
    public PurchasingDbContext Context => db;

    public DbSet<PurchaseMessage> Messages => db.Messages;
    public DbSet<PurchaseSourceRow> SourceRows => db.SourceRows;
    public DbSet<PurchaseSourceRevision> SourceRevisions => db.SourceRevisions;
    public DbSet<PurchaseMessageFile> Files => db.Files;
    public DbSet<PurchaseSyncState> SyncStates => db.SyncStates;
    public DbSet<PurchaseReviewChange> ReviewChanges => db.ReviewChanges;

    public async Task<IPurchaseWriteScope?> TryBeginWriteAsync(CancellationToken cancellationToken) =>
        LockHeld ? null : new Scope(await db.Database.BeginTransactionAsync(cancellationToken));

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);

    private sealed class Scope(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction) : IPurchaseWriteScope
    {
        private bool disposed;
        public Task CommitAsync(CancellationToken cancellationToken) => transaction.CommitAsync(cancellationToken);

        public async ValueTask DisposeAsync()
        {
            if (disposed) return;
            disposed = true;
            await transaction.DisposeAsync();
        }
    }
}

internal sealed class PurchasingTestHarness : IDisposable
{
    private readonly SqliteConnection connection;

    public PurchasingTestHarness()
    {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var context = new PurchasingDbContext(new DbContextOptionsBuilder<PurchasingDbContext>().UseSqlite(connection).Options);
        context.Database.EnsureCreated();
        Db = new SqlitePurchasingDb(context);
    }

    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 11, 5, 8, 0, 0, TimeSpan.Zero));
    public FakeSheetReader Sheet { get; } = new();
    public SqlitePurchasingDb Db { get; }
    public FakeSupplierReader Suppliers { get; } = new();

    public PurchaseRegisterSync Sync() => new(Db, Sheet, Time, NullLogger<PurchaseRegisterSync>.Instance);

    public ReviewPurchaseInvoiceCommandHandler Review() => new(Db, Suppliers, Time);

    /// <summary>A fresh context on the same database, so reads do not see tracked entities.</summary>
    public PurchasingDbContext Fresh() =>
        new(new DbContextOptionsBuilder<PurchasingDbContext>().UseSqlite(connection).Options);

    public void Dispose()
    {
        Db.Context.Dispose();
        connection.Dispose();
    }
}
