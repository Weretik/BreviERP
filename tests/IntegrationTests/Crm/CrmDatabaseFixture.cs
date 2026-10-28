using Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace IntegrationTests.Crm;

public abstract class CrmDatabaseFixture
{
    private string _database = null!;
    private string _adminConnection = null!;
    protected string Connection = null!;

    [OneTimeSetUp]
    public async Task CreateIsolatedDatabase()
    {
        var source = Environment.GetEnvironmentVariable("BREVIERP_CRM_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(source)) Assert.Ignore("Set BREVIERP_CRM_TEST_CONNECTION to local PostgreSQL for isolated CRM tests.");
        var builder = new NpgsqlConnectionStringBuilder(source);
        if (builder.Host is not ("localhost" or "127.0.0.1" or "::1")) throw new InvalidOperationException("Only local PostgreSQL is allowed.");
        builder.Database = "postgres";
        _adminConnection = builder.ConnectionString;
        _database = "brevi_crm_test_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(_adminConnection);
        await admin.OpenAsync();
        await using var create = new NpgsqlCommand($"CREATE DATABASE \"{_database}\"", admin);
        await create.ExecuteNonQueryAsync();
        builder.Database = _database;
        Connection = builder.ConnectionString;
        await using var db = Context();
        await db.Database.MigrateAsync();
        await db.Database.MigrateAsync();
    }

    [SetUp]
    public async Task ResetOwnDatabase()
    {
        if (!_database.StartsWith("brevi_crm_test_", StringComparison.Ordinal)) throw new InvalidOperationException();
        await using var db = Context();
        await db.Database.ExecuteSqlRawAsync(
            "TRUNCATE crm.\"Customers\", crm.\"MutationRequests\", crm.\"CalculationBases\", crm.\"QuoteMutationRequests\", crm.\"CustomSewingDrafts\", crm.\"CustomSewingCalculations\", crm.\"CustomSewingMutationRequests\", crm.\"CustomSewingTemplates\", crm.\"CustomSewingTemplateMutationRequests\" RESTART IDENTITY CASCADE");
    }

    [OneTimeTearDown]
    public async Task DropOwnDatabase()
    {
        if (string.IsNullOrWhiteSpace(_adminConnection) || string.IsNullOrWhiteSpace(_database)) return;
        if (!_database.StartsWith("brevi_crm_test_", StringComparison.Ordinal)) throw new InvalidOperationException();
        NpgsqlConnection.ClearAllPools();
        await using var admin = new NpgsqlConnection(_adminConnection);
        await admin.OpenAsync();
        await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_database}\" WITH (FORCE)", admin);
        await drop.ExecuteNonQueryAsync();
    }

    protected CrmDbContext Context() => new(new DbContextOptionsBuilder<CrmDbContext>()
        .UseNpgsql(Connection, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "crm")).Options);
}
