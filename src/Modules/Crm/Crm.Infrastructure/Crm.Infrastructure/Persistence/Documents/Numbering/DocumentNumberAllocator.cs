using Crm.Application.Documents.Numbering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using NpgsqlTypes;

namespace Crm.Infrastructure.Persistence.Documents;

public sealed class DocumentNumberAllocator(CrmDbContext db) : IDocumentNumberAllocator
{
    public async Task<string> AllocateAsync(DateTimeOffset readyAt, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Document number allocation requires the quote ready transaction.");
        var utc = readyAt.ToUniversalTime();
        var year = utc.Year;
        var month = utc.Month;
        var lockKey = 484933448000L + year * 100L + month;
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({lockKey})", ct);
        var existing = await MaxExistingAsync(year, month, ct);
        var sequence = await db.Set<DocumentNumberSequence>().SingleOrDefaultAsync(x => x.Year == year && x.Month == month, ct);
        var number = Math.Max(sequence?.NextNumber ?? 10, existing + 1);
        if (number == int.MaxValue) throw new InvalidOperationException("Document number sequence exhausted.");
        if (sequence is null)
        {
            sequence = new DocumentNumberSequence { Year = year, Month = month, NextNumber = number + 1 };
            db.Add(sequence);
        }
        else sequence.NextNumber = number + 1;
        await db.SaveChangesAsync(ct);
        return $"{number}/{month}/{year}";
    }

    private async Task<int> MaxExistingAsync(int year, int month, CancellationToken ct)
    {
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.Transaction = db.Database.CurrentTransaction!.GetDbTransaction();
        command.CommandText = """
            SELECT COALESCE(MAX((m.parts[1])::integer), 0)
            FROM (
                SELECT regexp_match("Number", '^([0-9]{1,9})[/-]([0-9]{1,2})[/-]([0-9]{4})$') AS parts
                FROM crm."Quotes" WHERE "Number" IS NOT NULL
            ) AS m
            WHERE m.parts IS NOT NULL
              AND m.parts[2]::integer = @month
              AND m.parts[3]::integer = @year
            """;
        command.Parameters.Add(new NpgsqlParameter("month", NpgsqlDbType.Integer) { Value = month });
        command.Parameters.Add(new NpgsqlParameter("year", NpgsqlDbType.Integer) { Value = year });
        var scalar = await command.ExecuteScalarAsync(ct);
        return scalar is int value ? value : Convert.ToInt32(scalar);
    }
}
