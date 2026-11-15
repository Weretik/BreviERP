using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Application;

/// <summary>
/// The save failures that mean «someone else was first» (database rules 4): a row version that moved or a unique index
/// that refused a second row. Any other <see cref="DbUpdateException"/> is a real error and must not be answered as a
/// stale page.
/// </summary>
public static class SaveRaces
{
    private const string PostgresUniqueViolation = "23505";
    private const string SqliteUniqueViolation = "UNIQUE constraint failed";

    /// <summary>A concurrency token moved or a unique index refused the row.</summary>
    public static bool IsRace(this DbUpdateException exception) =>
        exception is DbUpdateConcurrencyException || exception.IsUniqueViolation();

    /// <summary>A unique index refused the row (PostgreSQL 23505, or SQLite in the unit tests).</summary>
    public static bool IsUniqueViolation(this DbUpdateException exception) =>
        exception.InnerException is DbException inner
        && (inner.SqlState == PostgresUniqueViolation
            || inner.Message.Contains(SqliteUniqueViolation, StringComparison.Ordinal));
}
