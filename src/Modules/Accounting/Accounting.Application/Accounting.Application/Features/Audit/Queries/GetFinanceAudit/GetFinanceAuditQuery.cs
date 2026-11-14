using System.Globalization;
using System.Text.Json;
using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Reports.Shared;

namespace Accounting.Application.Features.Audit.Queries.GetFinanceAudit;

/// <summary>«Журнал змін»: changes of all finance records, newest first, 50 per page; optionally of one kind or action.</summary>
public sealed record GetFinanceAuditQuery(AuditEntity? Entity, AuditAction? Action, int Page)
    : IQuery<Result<FinanceAuditPageResponse>>;

public sealed class GetFinanceAuditValidator : AbstractValidator<GetFinanceAuditQuery>
{
    public GetFinanceAuditValidator()
    {
        RuleFor(x => x.Entity).IsInEnum().When(x => x.Entity is not null);
        RuleFor(x => x.Action).IsInEnum().When(x => x.Action is not null);
        RuleFor(x => x.Page).GreaterThan(0);
    }
}

public sealed class GetFinanceAuditHandler(IAccountingDbContext db)
    : IQueryHandler<GetFinanceAuditQuery, Result<FinanceAuditPageResponse>>
{
    public async ValueTask<Result<FinanceAuditPageResponse>> Handle(GetFinanceAuditQuery query,
        CancellationToken cancellationToken)
    {
        var entries = db.AuditEntries.AsNoTracking();
        if (query.Entity is { } entity) entries = entries.Where(x => x.Entity == entity);
        if (query.Action is { } action) entries = entries.Where(x => x.Action == action);
        var total = await entries.CountAsync(cancellationToken);
        var page = await entries.OrderByDescending(x => x.Id)
            .Skip((query.Page - 1) * FinanceJournal.PageSize).Take(FinanceJournal.PageSize)
            .ToListAsync(cancellationToken);
        return Result.Success(new FinanceAuditPageResponse(
            page.Select(x => new FinanceAuditEntryResponse(x.Id, x.Entity, x.EntityId, x.Action, x.By, x.At,
                AuditTitle.Of(x.AfterJson ?? x.BeforeJson), x.BeforeJson, x.AfterJson)).ToList(),
            total, query.Page, FinanceJournal.PageSize));
    }
}

/// <summary>A short name of the record from its snapshot.</summary>
internal static class AuditTitle
{
    public static string Of(string? snapshot)
    {
        if (string.IsNullOrWhiteSpace(snapshot)) return string.Empty;
        try
        {
            using var document = JsonDocument.Parse(snapshot);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return string.Empty;
            var name = Text(root, "name") ?? Text(root, "fileName");
            if (name is not null) return name;

            var parts = new List<string>();
            if (Text(root, "date") is { } date && DateOnly.TryParse(date, CultureInfo.InvariantCulture, out var day))
                parts.Add(day.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture));
            if (root.TryGetProperty("amountMinor", out var amount) && amount.TryGetInt64(out var minor))
                parts.Add($"{FinanceReportCsv.Money(minor)} {Text(root, "currency") ?? string.Empty}".TrimEnd());
            if (Text(root, "fromAccountName") is { } from && Text(root, "toAccountName") is { } to)
                parts.Add($"{from} → {to}");
            if ((Text(root, "description") ?? Text(root, "note")) is { } description) parts.Add(description);
            return string.Join(" · ", parts);
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    private static string? Text(JsonElement root, string property) =>
        root.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            && value.GetString() is { Length: > 0 } text
            ? text
            : null;
}
