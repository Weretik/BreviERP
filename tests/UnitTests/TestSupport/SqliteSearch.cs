using Microsoft.Data.Sqlite;

namespace UnitTests.TestSupport;

/// <summary>
/// SQLite lowers only Latin letters; PostgreSQL lowers Cyrillic too. Replacing <c>lower</c> with the .NET one makes the
/// generated <c>SearchText</c> columns behave in unit tests as in production.
/// </summary>
internal static class SqliteSearch
{
    public static SqliteConnection WithSearch(this SqliteConnection connection)
    {
        connection.CreateFunction("lower", (string? text) => text?.ToLowerInvariant(), isDeterministic: true);
        return connection;
    }
}
