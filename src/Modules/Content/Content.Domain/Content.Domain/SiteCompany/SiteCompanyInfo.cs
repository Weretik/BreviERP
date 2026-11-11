using System.Text.RegularExpressions;

namespace Content.Domain.SiteCompany;

/// <summary>A text in the two languages of the site.</summary>
public sealed record SiteText(string Uk, string Ru);

/// <summary>A client manager on the «Контакти» page, in the order staff set.</summary>
/// <param name="PhotoUrl">A picture of the «Фото» library (https) or a picture of the site itself (<c>/assets/…</c>).</param>
public sealed record SiteManager(SiteText Name, string? PhotoUrl, IReadOnlyList<string> Phones, string? Email);

/// <summary>Payment details from «Доставка та оплата» and the agreement.</summary>
public sealed record SiteBankDetails(SiteText Recipient, string Edrpou, string Iban, SiteText Bank);

/// <summary>The work schedule: the days (Monday = 1 … Sunday = 7) and one time range.</summary>
public sealed record SiteSchedule(IReadOnlyList<int> Days, string Opens, string Closes);

/// <summary>Profile links; null hides the link.</summary>
public sealed record SiteSocials(string? Instagram, string? Youtube, string? Facebook);

/// <summary>
/// «Контент → Контакти та компанія» (CNT-009): the company facts the public site shows in the header, footer,
/// «Контакти», «Доставка та оплата», the agreement and structured data. One row; the migration seeds what the site
/// showed before.
/// </summary>
public sealed partial class SiteCompanyInfo
{
    public const int SingletonId = 1;
    public const int TextMaxLength = 200;
    public const int EmailMaxLength = 120;
    public const int UrlMaxLength = 500;
    public const int MaxManagers = 10;
    public const int MaxManagerPhones = 3;

    private SiteCompanyInfo() { }

    public SiteCompanyInfo(string updatedBy, DateTimeOffset now)
    {
        Id = SingletonId;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = now;
    }

    public int Id { get; private set; }

    /// <summary>The one company phone of the header and the structured data, as people read it: <c>050 469 86 75</c>.</summary>
    public string Phone { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    public SiteText City { get; private set; } = null!;

    public SiteText Street { get; private set; } = null!;

    /// <summary>What the map link and the embedded map look for.</summary>
    public string MapQuery { get; private set; } = null!;

    public SiteSchedule Schedule { get; private set; } = null!;

    public IReadOnlyList<SiteManager> Managers { get; private set; } = [];

    public SiteBankDetails BankDetails { get; private set; } = null!;

    public SiteSocials Socials { get; private set; } = null!;

    public int Revision { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public string UpdatedBy { get; private set; } = null!;

    public void Replace(string phone, string email, SiteText city, SiteText street, string mapQuery,
        SiteSchedule schedule, IReadOnlyList<SiteManager> managers, SiteBankDetails bankDetails, SiteSocials socials,
        string updatedBy, DateTimeOffset now)
    {
        Phone = FormatPhone(phone) ?? throw new ArgumentException("Invalid phone.");
        Email = Clean(email) ?? throw new ArgumentException("Email is required.");
        City = Clean(city);
        Street = Clean(street);
        MapQuery = Clean(mapQuery) ?? throw new ArgumentException("Map query is required.");
        Schedule = new SiteSchedule(schedule.Days.Distinct().Order().ToArray(), schedule.Opens.Trim(), schedule.Closes.Trim());
        Managers = managers.Select(m => new SiteManager(Clean(m.Name), Clean(m.PhotoUrl),
            m.Phones.Select(p => FormatPhone(p) ?? throw new ArgumentException("Invalid phone.")).ToArray(),
            Clean(m.Email))).ToArray();
        BankDetails = new SiteBankDetails(Clean(bankDetails.Recipient), Digits(bankDetails.Edrpou),
            FormatIban(bankDetails.Iban) ?? throw new ArgumentException("Invalid IBAN."), Clean(bankDetails.Bank));
        Socials = new SiteSocials(Clean(socials.Instagram), Clean(socials.Youtube), Clean(socials.Facebook));
        Revision++;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = now;
    }

    public static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static SiteText Clean(SiteText text) => new(Clean(text.Uk) ?? string.Empty, Clean(text.Ru) ?? string.Empty);

    private static string Digits(string value) => new(value.Where(char.IsAsciiDigit).ToArray());

    /// <summary>
    /// A Ukrainian number in any common form (<c>0504698675</c>, <c>+380 50 469 86 75</c>, <c>(050) 469-86-75</c>)
    /// as the site shows it, <c>050 469 86 75</c>; null for anything else.
    /// </summary>
    public static string? FormatPhone(string? value)
    {
        if (value is null) return null;
        var digits = Digits(value);
        if (digits.Length == 12 && digits.StartsWith("380", StringComparison.Ordinal)) digits = digits[2..];
        if (digits.Length != 10 || digits[0] != '0') return null;
        return $"{digits[..3]} {digits[3..6]} {digits[6..8]} {digits[8..]}";
    }

    /// <summary>The <c>tel:</c> form of a formatted number: <c>+380504698675</c>.</summary>
    public static string PhoneE164(string formatted) => "+38" + Digits(formatted);

    /// <summary>A Ukrainian IBAN (UA and 27 digits) grouped as banks print it; null for anything else.</summary>
    public static string? FormatIban(string? value)
    {
        if (value is null) return null;
        var compact = new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();
        if (!Iban().IsMatch(compact)) return null;
        return $"{compact[..4]} {compact[4..10]} {compact[10..15]} {compact[15..]}";
    }

    /// <summary>ЄДРПОУ of a company (8 digits) or a sole trader's tax number (10 digits).</summary>
    public static bool IsEdrpou(string? value) => value is not null && Edrpou().IsMatch(value.Trim());

    public static bool IsTime(string? value) => value is not null && Time().IsMatch(value.Trim());

    public static bool IsEmail(string? value) => Clean(value) is { } email && email.Length <= EmailMaxLength && Email_().IsMatch(email);

    public static bool IsHttpsUrl(string? value) => Clean(value) is not { } url
        || (url.Length <= UrlMaxLength && Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps);

    /// <summary>A picture of the library (https) or of the site itself (<c>/assets/…</c>).</summary>
    public static bool IsPhotoUrl(string? value) => Clean(value) is not { } url
        || (url.StartsWith("/assets/", StringComparison.Ordinal) && url.Length <= UrlMaxLength) || IsHttpsUrl(url);

    [GeneratedRegex("^UA\\d{27}$")]
    private static partial Regex Iban();

    [GeneratedRegex("^\\d{8}(\\d{2})?$")]
    private static partial Regex Edrpou();

    [GeneratedRegex("^([01]\\d|2[0-3]):[0-5]\\d$")]
    private static partial Regex Time();

    [GeneratedRegex("^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$")]
    private static partial Regex Email_();
}
