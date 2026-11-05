namespace Production.Domain.Archive;

/// <summary>Which old book a record of «Архів цеху» comes from.</summary>
public enum WorkshopArchiveKind
{
    /// <summary>The Google sheet «ГРАФИК работы», 2017–2026.</summary>
    Schedule = 1,

    /// <summary>The requests of the old «Цех» before it kept orders (its legacy archive).</summary>
    Request = 2
}

/// <summary>
/// A read-only line of «Архів цеху» moved from the old «Цех» (PRD-D54). Nothing edits it; a repeat import of the same
/// <see cref="SourceKey"/> is skipped.
/// </summary>
public sealed class WorkshopArchiveRecord
{
    public const int SourceKeyMaxLength = 80;
    public const int ShortMaxLength = 200;
    public const int TextMaxLength = 2000;
    public const int SearchMaxLength = 8000;

    private List<WorkshopArchiveField> _fields = [];
    private List<WorkshopArchiveLink> _links = [];

    private WorkshopArchiveRecord()
    {
    }

    public WorkshopArchiveRecord(WorkshopArchiveKind kind, string sourceKey, int sourceRow, WorkshopArchiveEntry entry)
    {
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        if (sourceRow <= 0) throw new ArgumentOutOfRangeException(nameof(sourceRow));
        Kind = kind;
        SourceKey = ProductionText.Clean(sourceKey, SourceKeyMaxLength);
        SourceRow = sourceRow;
        Date = entry.Date;
        DateText = Optional(entry.DateText, ShortMaxLength);
        Number = Optional(entry.Number, ShortMaxLength);
        Client = Optional(entry.Client, ShortMaxLength);
        Product = Optional(entry.Product, TextMaxLength);
        Quantity = Optional(entry.Quantity, ShortMaxLength);
        Fabric = Optional(entry.Fabric, ShortMaxLength);
        Status = Optional(entry.Status, ShortMaxLength);
        Notes = Optional(entry.Notes, TextMaxLength);
        _fields = [.. entry.Fields.Where(x => !string.IsNullOrWhiteSpace(x.Label) && !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => new WorkshopArchiveField(x.Label.Trim(), x.Value.Trim()))];
        _links = [.. entry.Links.Where(x => !string.IsNullOrWhiteSpace(x.Url))
            .Select(x => new WorkshopArchiveLink(x.Label.Trim(), x.Url.Trim()))];
        var search = string.Join('\n', new[] { DateText, Number, Client, Product, Quantity, Fabric, Status, Notes }
            .Concat(_fields.Select(x => x.Value)).Where(x => !string.IsNullOrWhiteSpace(x))).ToLowerInvariant();
        SearchText = search.Length > SearchMaxLength ? search[..SearchMaxLength] : search;
    }

    public int Id { get; private set; }
    public WorkshopArchiveKind Kind { get; private set; }

    /// <summary>«schedule:12» for a sheet row, the old id for a request.</summary>
    public string SourceKey { get; private set; } = null!;

    public int SourceRow { get; private set; }
    public DateOnly? Date { get; private set; }

    /// <summary>The date as written when it is not a date.</summary>
    public string? DateText { get; private set; }

    public string? Number { get; private set; }
    public string? Client { get; private set; }
    public string? Product { get; private set; }

    /// <summary>Free text such as «10+4».</summary>
    public string? Quantity { get; private set; }

    public string? Fabric { get; private set; }
    public string? Status { get; private set; }
    public string? Notes { get; private set; }
    public IReadOnlyList<WorkshopArchiveField> Fields => _fields;
    public IReadOnlyList<WorkshopArchiveLink> Links => _links;

    /// <summary>Lower-case text of the record for «Пошук».</summary>
    public string SearchText { get; private set; } = null!;

    private static string? Optional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var text = value.Trim();
        return text.Length > maxLength ? text[..maxLength] : text;
    }
}

/// <summary>What an archive line holds, as read from the old book.</summary>
public sealed record WorkshopArchiveEntry(
    DateOnly? Date, string? DateText, string? Number, string? Client, string? Product, string? Quantity,
    string? Fabric, string? Status, string? Notes, IReadOnlyList<WorkshopArchiveField> Fields,
    IReadOnlyList<WorkshopArchiveLink> Links);

/// <summary>Another column of the old book, such as «Сезон» or «Пошив».</summary>
public sealed record WorkshopArchiveField(string Label, string Value);

public sealed record WorkshopArchiveLink(string Label, string Url);
