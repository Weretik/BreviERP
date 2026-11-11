namespace Crm.Domain.SiteRequests;

/// <summary>
/// A request a visitor left on the «Заявка» form of the public site (LEAD-002): a phone and optionally what to sew,
/// how many, a comment and a file. A manager takes it, calls back and closes it with a note.
/// <see cref="Version"/> grows with every change and guards against two managers at once.
/// </summary>
public sealed class SiteRequest
{
    public const int PhoneMaxLength = 32;
    public const int WorkwearMaxLength = 200;
    public const int QuantityMaxLength = 60;
    public const int CommentMaxLength = 1000;
    public const int FileNameMaxLength = 200;
    public const int UrlMaxLength = 500;
    public const int NameMaxLength = 200;
    public const int NoteMaxLength = 1000;
    public const int ErrorMaxLength = 300;

    private SiteRequest() { }

    public SiteRequest(string phone, string? workwear, string? quantity, string? comment, string locale,
        string? fileName, string? fileUrl, DateTimeOffset now)
    {
        Phone = Cut(phone, PhoneMaxLength) ?? throw new ArgumentException("A phone is required.", nameof(phone));
        Workwear = Cut(workwear, WorkwearMaxLength);
        Quantity = Cut(quantity, QuantityMaxLength);
        Comment = Cut(comment, CommentMaxLength);
        Locale = locale == "ru" ? "ru" : "uk";
        FileName = fileUrl is null ? null : Cut(fileName, FileNameMaxLength);
        FileUrl = Cut(fileUrl, UrlMaxLength);
        Status = SiteRequestStatus.New;
        CreatedAt = now;
        Version = 1;
    }

    public long Id { get; private set; }
    public string Phone { get; private set; } = null!;
    public string? Workwear { get; private set; }
    public string? Quantity { get; private set; }
    public string? Comment { get; private set; }

    /// <summary>The language of the page the visitor used: `uk` or `ru`.</summary>
    public string Locale { get; private set; } = "uk";

    public string? FileName { get; private set; }

    /// <summary>The attached file on the CDN.</summary>
    public string? FileUrl { get; private set; }

    public SiteRequestStatus Status { get; private set; }
    public int? OwnerUserId { get; private set; }
    public string? OwnerName { get; private set; }
    public DateTimeOffset? TakenAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }
    public string? Note { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>When the bot posted the request to the chosen groups; null when it did not.</summary>
    public DateTimeOffset? NotifiedAt { get; private set; }

    /// <summary>Why the bot could not post it, for staff.</summary>
    public string? NotifyError { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }
    public int Version { get; private set; }

    public bool IsOpen => Status is SiteRequestStatus.New or SiteRequestStatus.InWork;

    public void Notified(DateTimeOffset? at, string? error)
    {
        NotifiedAt = at;
        NotifyError = Cut(error, ErrorMaxLength);
    }

    /// <summary>«Беру»: false when another manager already holds it.</summary>
    public bool Take(int userId, string name, DateTimeOffset now)
    {
        if (!IsOpen) return false;
        if (OwnerUserId is not null && OwnerUserId != userId) return false;
        OwnerUserId = userId;
        OwnerName = Cut(name, NameMaxLength);
        TakenAt ??= now;
        Status = SiteRequestStatus.InWork;
        Touch(now);
        return true;
    }

    /// <summary>«Готово» or «Не заявка» with what came of it; the closer becomes the owner when nobody was.</summary>
    public void Close(SiteRequestStatus status, int userId, string name, string? note, DateTimeOffset now)
    {
        if (status is not (SiteRequestStatus.Done or SiteRequestStatus.Spam))
            throw new ArgumentException("A request closes as done or spam.", nameof(status));
        if (OwnerUserId is null)
        {
            OwnerUserId = userId;
            OwnerName = Cut(name, NameMaxLength);
            TakenAt = now;
        }

        Status = status;
        Note = Cut(note, NoteMaxLength);
        ClosedAt = now;
        Touch(now);
    }

    /// <summary>«Повернути в нові»: frees the request again.</summary>
    public void Reopen(DateTimeOffset now)
    {
        Status = SiteRequestStatus.New;
        OwnerUserId = null;
        OwnerName = null;
        TakenAt = null;
        ClosedAt = null;
        Touch(now);
    }

    private void Touch(DateTimeOffset now)
    {
        UpdatedAt = now;
        Version++;
    }

    public static string? Cut(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var text = string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (value.Contains('\n')) text = string.Join('\n', value.Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0));
        return text.Length <= max ? text : text[..max];
    }
}
