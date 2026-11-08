using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.Unicode;
using Accounting.Application.Abstractions;
using Accounting.Domain.Ledger;

namespace Accounting.Application.Export;

/// <summary>
/// «Резервна копія»: every finance record in one JSON file — accounts, categories, counterparties, operations and
/// transfers (cancelled ones too), file details (not the files), the change log and the bank statements with their lines.
/// </summary>
public sealed record GetFinanceExportQuery : IQuery<Result<FinanceExportFile>>;

/// <summary>The JSON file and its name, <c>brevi-finance-YYYY-MM-DD.json</c> by the Kyiv day.</summary>
public sealed record FinanceExportFile(byte[] Content, string FileName);

public sealed class GetFinanceExportQueryHandler(IAccountingDbContext db, TimeProvider clock)
    : IQueryHandler<GetFinanceExportQuery, Result<FinanceExportFile>>
{
    public const string Format = "brevi-erp-finance-v1";

    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById(LedgerCalendar.TimeZoneId);

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        Converters = { new JsonStringEnumConverter() },
        // Stored fields only: properties computed from them (details, links, states) are left out.
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { StoredOnly } },
    };

    public async ValueTask<Result<FinanceExportFile>> Handle(GetFinanceExportQuery query, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        using var content = new MemoryStream();
        await using (var writer = new Utf8JsonWriter(content, new JsonWriterOptions { Indented = false, Encoder = Json.Encoder }))
        {
            writer.WriteStartObject();
            writer.WriteString("format", Format);
            writer.WriteString("exportedAt", now);
            writer.WriteString("timezone", LedgerCalendar.TimeZoneId);
            writer.WriteString("cutoverDate", LedgerCalendar.CutoverDate.ToString("yyyy-MM-dd"));
            await WriteAsync(writer, "accounts", db.Accounts.AsNoTracking().OrderBy(x => x.SortOrder), cancellationToken);
            await WriteAsync(writer, "categories", db.Categories.AsNoTracking().OrderBy(x => x.Id), cancellationToken);
            await WriteAsync(writer, "counterparties", db.Counterparties.AsNoTracking().OrderBy(x => x.Id), cancellationToken);
            await WriteAsync(writer, "transactions", db.Transactions.AsNoTracking().OrderBy(x => x.Id), cancellationToken);
            await WriteAsync(writer, "transfers", db.Transfers.AsNoTracking().OrderBy(x => x.Id), cancellationToken);
            await WriteAsync(writer, "attachments", db.Attachments.AsNoTracking().OrderBy(x => x.Id), cancellationToken);
            await WriteAsync(writer, "auditEntries", db.AuditEntries.AsNoTracking().OrderBy(x => x.Id), cancellationToken);
            await WriteAsync(writer, "bankStatements", db.BankStatements.AsNoTracking().OrderBy(x => x.Id), cancellationToken);
            await WriteAsync(writer, "bankStatementLines",
                db.BankStatementLines.AsNoTracking().OrderBy(x => x.StatementId).ThenBy(x => x.RowNumber), cancellationToken);
            writer.WriteEndObject();
        }

        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, Kyiv).DateTime);
        return Result.Success(new FinanceExportFile(content.ToArray(), $"brevi-finance-{today:yyyy-MM-dd}.json"));
    }

    private static async Task WriteAsync<T>(Utf8JsonWriter writer, string name, IQueryable<T> rows,
        CancellationToken cancellationToken)
    {
        writer.WritePropertyName(name);
        JsonSerializer.Serialize(writer, await rows.ToListAsync(cancellationToken), Json);
        await writer.FlushAsync(cancellationToken);
    }

    private static void StoredOnly(JsonTypeInfo info)
    {
        if (info.Kind != JsonTypeInfoKind.Object || info.Type.Namespace?.StartsWith("Accounting.Domain", StringComparison.Ordinal) != true)
            return;
        for (var i = info.Properties.Count - 1; i >= 0; i--)
            if (info.Properties[i].AttributeProvider is PropertyInfo { SetMethod: null })
                info.Properties.RemoveAt(i);
    }
}
