using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Production.Application.Abstractions;
using Production.Infrastructure.Persistence;

namespace UnitTests.Production;

internal sealed class FakeProductionModelReader : IProductionModelReader
{
    public List<ProductionModel> Models { get; } = [];

    public Task<IReadOnlyList<ProductionModel>> ListSewingModelsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ProductionModel>>(Models);
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

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
