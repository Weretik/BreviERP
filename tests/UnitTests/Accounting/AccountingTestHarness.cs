using Accounting.Application.Accounts;
using Accounting.Domain.Accounts;
using Accounting.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;

namespace UnitTests.Accounting;

/// <summary>The real module DbContext on in-memory SQLite.</summary>
internal sealed class AccountingTestHarness : IDisposable
{
    public const string Director = "director@brevi.com.ua";
    private readonly SqliteConnection connection;

    public AccountingTestHarness()
    {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        Db = new AccountingDbContext(new DbContextOptionsBuilder<AccountingDbContext>().UseSqlite(connection).Options);
        Db.Database.EnsureCreated();
    }

    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 11, 5, 8, 0, 0, TimeSpan.Zero));
    public AccountingDbContext Db { get; }

    public static FinanceAccountFields Fields(string name = "ПриватБанк ФОП", long openingMinor = 1_000_00,
        string? iban = "ua21 3052 9900 0002 6007 0000 0000 1", int sortOrder = 10) =>
        new(name, FinanceAccountType.Bank, "uah", iban, openingMinor, new DateOnly(2024, 1, 1), null, sortOrder);

    /// <summary>A fresh context on the same database, so reads do not see tracked entities.</summary>
    public AccountingDbContext Fresh() =>
        new(new DbContextOptionsBuilder<AccountingDbContext>().UseSqlite(connection).Options);

    public void Dispose()
    {
        Db.Dispose();
        connection.Dispose();
    }
}
